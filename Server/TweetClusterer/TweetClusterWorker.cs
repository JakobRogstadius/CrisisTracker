/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrisisTracker.Common;
using System.Globalization;

namespace CrisisTracker.TweetClusterer
{
    public class TweetClusterWorker
    {
        class TweetRelationSimple
        {
            public long TweetID { get; set; }
            public long TweetID2 { get; set; }
            public double Similarity { get; set; }
        }

        class TweetNeighbor
        {
            public long TweetID { get; set; }
            public long? TweetClusterID { get; set; }
            public int Degree { get; set; }
        }

        const string Name = "TweetClusterWorker";
        //int _wordsPerHyperPlane = 1 + (int)(Math.Log(Math.Pow(Settings.TweetClusterer_TCW_DictionaryWordCount, 2)));

        List<LSHashTable> _tables = new List<LSHashTable>();
        LSHashTweetHistory _history = new LSHashTweetHistory(Settings.TweetClusterer_TCW_HistorySize);
        Random _random = new Random();

        int _nextTableRehashIndex = 0;

        public int ProcessTweetBatch()
        {
            try
            {
                Console.Write("Fetching tweets...");
                List<LSHashTweet> tweets = GetTweetBatch(Settings.TweetClusterer_TCW_BatchSize, getProcessed: false);
                Console.WriteLine(" (" + tweets.Count + ")");
                if (tweets.Count == 0)
                    return 0;

                Dictionary<long, double> maxSimilarities = new Dictionary<long, double>();
                List<TweetRelationSimple> relations = new List<TweetRelationSimple>();

                //Process each tweet
                Console.WriteLine("Processing tweets...");
                foreach (LSHashTweet t in tweets)
                {
                    //Skip content-less tweets
                    if (t.Vector.ItemCount < Settings.TweetClusterer_TCW_MinTweetWordCount
                        || t.Vector.OriginalLength < Settings.TweetClusterer_TCW_MinTweetVectorLength)
                    {
                        continue;
                    }

                    //Add the tweet to each hash table and keep track of collisions with previously added tweets
                    Dictionary<LSHashTweet, int> neighborCandidates = GetNeighborCandidates(t);

                    //Look for relations in hash bin collisions
                    double maxSimilarity = 0;
                    maxSimilarity = GetNeighbors(relations, t, neighborCandidates, maxSimilarity);

                    //If retweet, add relation to parent
                    if (t.RetweetOf.HasValue && !relations.Any(n => n.TweetID2 == t.RetweetOf.Value))
                    {
                        relations.Add(new TweetRelationSimple() {
                            TweetID = t.ID, 
                            TweetID2 = t.RetweetOf.Value, 
                            Similarity = Settings.TweetClusterer_TCW_IdentityThreshold 
                        });
                        maxSimilarity = Math.Max(maxSimilarity, Settings.TweetClusterer_TCW_IdentityThreshold);
                    }

                    //If no neighbor from bins, search in history
                    if (relations.Count < 2)
                    {
                        maxSimilarity = GetNeighborsFromRecentHistory(relations, t, maxSimilarity);
                    }

                    if (maxSimilarity < Settings.TweetClusterer_TCW_IdentityThreshold)
                    {
                        _history.Add(t); //Only add tweets that were "unique" or "novel" according to the hash tables
                    }

                    maxSimilarities.Add(t.ID, maxSimilarity);
                }

                IEnumerable<long> allTweetIDs = _tables.SelectMany(n => n.GetTweetIDs()).Distinct();
                Console.WriteLine("Unique items in all hash tables: " + allTweetIDs.Count());

                Console.WriteLine("Storing relations... ");
                StoreRelations(relations);

                Console.Write("Assigning cluster IDs... ");
                AssignTweetClusterIDsToBatchTweets(tweets, relations, maxSimilarities);
                Console.WriteLine();

                Console.WriteLine("Correcting novelty scores...");
                CorrectNoveltyScores(tweets);

                Console.WriteLine("Marking tweets as processed...");
                MarkTweetsAsProcessed(tweets);

                return tweets.Count;
            }
            catch (Exception e)
            {
                Output.Print(Name, e);
                throw e;
            }
        }

        List<LSHashTweet> GetTweetBatch(int n, bool getProcessed)
        {
            string sql = @"select
                    T.TweetID,
                    T.RetweetOf,
                    WordTweet.WordID,
                    ScoreToIdf(Score4d) as WordWeight
                from
                    (select TweetID, RetweetOf from Tweet
                    " + (getProcessed ?
                        "where CalculatedRelations order by TweetID desc" :
                        "where not CalculatedRelations order by TweetID") + @"
                    limit " + n + @") T
                    left join WordTweet on WordTweet.TweetID = T.TweetID
                    left join WordScore on WordScore.WordID = WordTweet.WordID
                order by TweetID;";

            Dictionary<long, LSHashTweet> tweets = new Dictionary<long, LSHashTweet>();
            Helpers.RunSelect(Name, sql, tweets,
                (values, reader) => AddTweetRow(
                    values,
                    Convert.ToInt64(reader["TweetID"]),
                    (reader["RetweetOf"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["RetweetOf"])),
                    (reader["WordID"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["WordID"])),
                    (reader["WordWeight"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["WordWeight"]))));

            foreach (LSHashTweet tweet in tweets.Values)
                tweet.Vector.Normalize();
            return tweets.Values.ToList();
        }

        void AddTweetRow(Dictionary<long, LSHashTweet> tweets, long tweetID, long? retweetOf, long? wordID, double? wordWeight)
        {
            if (!tweets.ContainsKey(tweetID))
                tweets.Add(tweetID, new LSHashTweet() { ID = tweetID, RetweetOf = retweetOf, Vector = new WordVector() });
            if (wordID.HasValue && wordWeight.HasValue)
                tweets[tweetID].Vector.AddItem(wordID.Value, wordWeight.Value);
        }

        private Dictionary<LSHashTweet, int> GetNeighborCandidates(LSHashTweet t)
        {
            Dictionary<LSHashTweet, int> neighborCandidates = new Dictionary<LSHashTweet, int>();
            foreach (LSHashTable table in _tables)
            {
                bool dummy;
                List<LSHashTweet> hits = table.Add(t, out dummy);
                foreach (LSHashTweet hit in hits)
                {
                    if (neighborCandidates.ContainsKey(hit))
                        neighborCandidates[hit]++;
                    else
                        neighborCandidates.Add(hit, 1);
                }
            }
            return neighborCandidates;
        }

        private static double GetNeighbors(List<TweetRelationSimple> relations, LSHashTweet t, Dictionary<LSHashTweet, int> neighborCandidates, Double maxSimilarity)
        {
            if (neighborCandidates.Count > 0)
            {
                IEnumerable<LSHashTweet> nearestCandidates = neighborCandidates
                    .OrderByDescending(n => n.Value) //sort by number of times the candidate was in the same bin as this tweet
                    .Select(n => n.Key) //select the candidates
                    .Take(3 * Settings.TweetClusterer_TCW_HashTableCount) //take the 3L best candidates
                    .OrderByDescending(n => n.Vector * t.Vector) //sort by actual similarity
                    .Take(Settings.TweetClusterer_TCW_MaxLinksPerTweet); //take best
                foreach (LSHashTweet candidate in nearestCandidates)
                {
                    double similarity = candidate.Vector * t.Vector;
                    if (similarity > maxSimilarity)
                        maxSimilarity = similarity;
                    if (similarity > Settings.TweetClusterer_TCW_MinTweetSimilarityForLink && t.ID != candidate.ID)
                    {
                        relations.Add(new TweetRelationSimple() { TweetID = t.ID, TweetID2 = candidate.ID, Similarity = similarity });
                    }
                }
            }
            return maxSimilarity;
        }

        private Double GetNeighborsFromRecentHistory(List<TweetRelationSimple> relations, LSHashTweet t, Double maxSimilarity)
        {
            List<LSHashTweet> nearestCandidates = _history.GetNearestNeighbors(t, Settings.TweetClusterer_TCW_MaxLinksPerTweet - relations.Count);
            foreach (LSHashTweet candidate in nearestCandidates)
            {
                double similarity = candidate.Vector * t.Vector;
                if (similarity > Settings.TweetClusterer_TCW_MinTweetSimilarityForLink)
                {
                    relations.Add(new TweetRelationSimple() { TweetID = t.ID, TweetID2 = candidate.ID, Similarity = similarity });

                    if (similarity > maxSimilarity)
                        maxSimilarity = similarity;
                }
            }
            return maxSimilarity;
        }

        void StoreRelations(List<TweetRelationSimple> relations)
        {
            if (relations.Count == 0)
                return;

            //Insert edges
            StringBuilder sqlSB = new StringBuilder();
            sqlSB.Append("insert ignore into TweetRelationInsertBuffer (TweetID1, TweetID2, Similarity) values ");
            bool first = true;
            foreach (TweetRelationSimple relation in relations)
            {
                if (first)
                    first = false;
                else
                    sqlSB.Append(',');
                sqlSB.Append('(');
                sqlSB.Append(relation.TweetID);
                sqlSB.Append(',');
                sqlSB.Append(relation.TweetID2);
                sqlSB.Append(',');
                sqlSB.Append(relation.Similarity.ToString(CultureInfo.InvariantCulture));
                sqlSB.Append(')');
            }
            sqlSB.Append(';');
            Helpers.RunSqlStatement(Name, sqlSB.ToString(), false);
            Helpers.RunSqlStatement(Name, "insert ignore into TweetRelation select b.* from TweetRelationInsertBuffer b join Tweet t1 on t1.TweetID=b.TweetID1 join Tweet t2 on t2.TweetID=b.TweetID2;", false);
            Helpers.RunSqlStatement(Name, "delete from TweetRelationInsertBuffer;", false);
        }

        private void AssignTweetClusterIDsToBatchTweets(List<LSHashTweet> tweets, List<TweetRelationSimple> relations, Dictionary<long, double> maxSimilarities)
        {
            if (tweets.Count == 0)
                return;

            //Allow dirty reads
            Helpers.RunSqlStatement(Name, "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", false);

            //Get the next unused cluster ID from the database
            long? nextClusterID = 0;
            Helpers.RunSelect(Name,
                "select coalesce(max(TweetClusterID)+1,0) as ID from TweetCluster",
                nextClusterID,
                (dummy, reader) => nextClusterID = Convert.ToInt64(reader["ID"]));

            //Get cluster IDs for all tweets to which the tweets in the batch have links, plus the degree of the target nodes
            string batchIDsStr = string.Join(",", tweets.Select(n => n.ID.ToString()).ToArray());
            Dictionary<long, TweetNeighbor> neighborInfo = new Dictionary<long, TweetNeighbor>();
            Helpers.RunSelect(Name,
                @"select 
                    T.TweetID, T.TweetClusterID, count(*) as Degree
                from (
                    select distinct t.TweetID, t.TweetClusterID
                    from TweetRelation tr join Tweet t on t.TweetID = tr.TweetID2
                    where TweetID1 in (" + batchIDsStr + @")
                ) T
                join TweetRelation tr on tr.TweetID1 = T.TweetID or tr.TweetID2 = T.TweetID
                group by T.TweetID",
                neighborInfo,
                (values, reader) =>
                    values.Add(
                        reader.GetInt64("TweetID"),
                        new TweetNeighbor()
                        {
                            TweetID = Convert.ToInt64(reader["TweetID"]),
                            TweetClusterID = (reader["TweetClusterID"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["TweetClusterID"])),
                            Degree = Convert.ToInt32(reader["Degree"])
                        }));
            Console.WriteLine("neighborInfo length: " + neighborInfo.Count);

            List<long> createdClusterIDs = new List<long>();
            Dictionary<long, long> assignedTweetClusterIDs = new Dictionary<long, long>(); //tweetID, clusterID

            //Group the edges by TweetID1 (tweet in batch)
            var tweetsWithRelations =
                from edge in relations
                group edge by edge.TweetID into tweetEdges
                select new { TweetID = tweetEdges.Key, Edges = tweetEdges };

            //Assign cluster IDs to those tweets for which no relations were found, or for which none of the target tweets exists
            foreach (LSHashTweet tweet in tweets.Where(n => 
                !tweetsWithRelations.Any(m => m.TweetID == n.ID)
                || !relations.Any(m => m.TweetID == n.ID && neighborInfo.ContainsKey(m.TweetID2))
                ))
            {
                assignedTweetClusterIDs.Add(tweet.ID, nextClusterID.Value);
                createdClusterIDs.Add(nextClusterID.Value);
                UpdateNeighbor(neighborInfo, tweet.ID, nextClusterID.Value);
                nextClusterID++;
            }
            Console.Write(".");
            
            //Assign cluster ID to each tweet in the batch: the majority ID in the local neighborhood if available, else the next unused ID
            HashSet<ValuePair<long, long>> clusterCollisions = new HashSet<ValuePair<long,long>>();
            foreach (var tweet in tweetsWithRelations)
            {
                //Get the tweets that this tweet is similar to
                var neighbors = tweet.Edges
                    .Where(e => neighborInfo.ContainsKey(e.TweetID2))
                    .Select(e => neighborInfo[e.TweetID2])
                    .ToList();
                if (!neighbors.Any()) //I think this happens when edges were added for retweets, but the source tweet was never seen
                    continue;

                //Rank the clusterIDs of the neighbors and pick the most frequent
                var idGroups =
                    (from n in neighbors
                     group n by n.TweetClusterID into g
                     where g.Key.HasValue
                     select new { ID = g.Key, Members = g, Score = g.Sum(m => 1 + 0.3 * Math.Log(m.Degree)) })
                    .OrderByDescending(n => n.Score);
                long topGroup = idGroups.First().ID.Value;

                //Assign the majority cluster ID to this tweet
                assignedTweetClusterIDs.Add(tweet.TweetID, topGroup);
                    
                //If the tweet bridges multiple clusters, make a record of this to later test merging of the clusters
                if (idGroups.Count() > 0)
                {
                    long minID = idGroups.Min(n => n.ID).Value;
                    var collisions = idGroups.Where(n => n.ID != minID).Select(n => new ValuePair<long, long>(minID, n.ID.Value));
                    clusterCollisions.UnionWith(collisions);
                }

                //Update this tweet's cluster index in the list of neighbors, as other tweets in the batch may link to this tweet
                UpdateNeighbor(neighborInfo, tweet.TweetID, assignedTweetClusterIDs[tweet.TweetID]);
            }

            //Insert new TweetClusters (otherwise the tweet update below will fail from trigger constraints)
            string tweetClusterInsert = "INSERT INTO TweetCluster (TweetClusterID, StartTime, EndTime, PendingClusterUpdate, PendingStoryUpdate) VALUES "
                + string.Join(",", assignedTweetClusterIDs.Values.Distinct().Select(n => "(" + n + ",'3000-01-01',0,1,1)"))
                + " ON DUPLICATE KEY UPDATE PendingClusterUpdate=1, PendingStoryUpdate=1";
            Helpers.RunSqlStatement(Name, tweetClusterInsert, false);
            Console.Write(".");

            //Save cluster assignments in DB (and save novelty score)
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO Tweet (TweetID, TweetClusterID, Novelty) VALUES ");
            bool first = true;
            foreach (var item in assignedTweetClusterIDs)
            {
                if (first) first = false;
                else sb.AppendLine(",");
                double novelty = 1;
                if (maxSimilarities.ContainsKey(item.Key)) //items are absent from the list if no neighbors were found
                    novelty = 1 - maxSimilarities[item.Key];
                sb.Append("(" + item.Key + "," + item.Value + "," + novelty + ")");
            }
            sb.AppendLine();
            sb.AppendLine("ON DUPLICATE KEY UPDATE TweetClusterID=VALUES(TweetClusterID), Novelty=VALUES(Novelty)");
            Helpers.RunSqlStatement(Name, sb.ToString(), false);
            Console.Write(".");

            //Update stats for all TweetClusters that recieved new tweets or which are sliding out of the 4h time window
            string sqlTweetClusterCounts = @"
                insert into TweetCluster (TweetClusterID, StartTime, EndTime, TweetCount)
                select TweetClusterID,
                    min(CreatedAt) as StartTime,
                    max(CreatedAt) as EndTime,
                    count(*) as TweetCount
                from
                    TweetCluster
                    natural join Tweet
                    join TwitterUser on TwitterUser.UserID=Tweet.UserID and not IsBlacklisted,
                    (select max(CreatedAt) - interval 4 hour as T4h from Tweet where CalculatedRelations) as TTT
                where 
		            PendingClusterUpdate
		            or T4h between StartTime and EndTime
                group by TweetClusterID
                on duplicate key update
                    StartTime=VALUES(StartTime),
                    EndTime=VALUES(EndTime),
                    TweetCount=VALUES(TweetCount)
                ";
            Helpers.RunSqlStatement(Name, sqlTweetClusterCounts, false);
            Console.Write(".");

            //Update TweetCluster titles
            string sqlTweetClusterTitles = @"
                update TweetCluster tc
                set Title = (
	                select IF(Text REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2), Text) Title
	                from Tweet t
	                join TwitterUser tu on tu.UserID=t.UserID
	                where t.TweetClusterID=tc.TweetClusterID and not IsBlacklisted
	                group by TextHash
	                order by count(*) desc
                    limit 1
                )
                where PendingClusterUpdate and not IsArchived
                ";
            Helpers.RunSqlStatement(Name, sqlTweetClusterTitles, false);
            Console.Write(".");

            //Reset PendingClusterUpdate
            Helpers.RunSqlStatement(Name, "update TweetCluster set PendingClusterUpdate=0 where PendingClusterUpdate", false);

            //Insert cluster collisions
            string sqlTweetClusterCollisions = "insert ignore into TweetClusterCollision (TweetClusterID1, TweetClusterID2) values "
                + String.Join(",", clusterCollisions.Select(n => "(" + n.Value1 + "," + n.Value1 + ")"));

            //Reset dirty reads
            Helpers.RunSqlStatement(Name, "SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ", false);
        }

        private void CorrectNoveltyScores(List<LSHashTweet> tweets)
        {
            if (tweets.Count == 0)
                return;

            string tweetIDsStr = String.Join(",", tweets.Select(n => n.ID));
            string sql = 
                @"update Tweet t1 
                join Tweet t2 
                    on t1.TextHash = t2.TextHash 
                    and t1.TweetID < t2.TweetID 
                    and t1.CreatedAt > t2.CreatedAt - interval 1 day
                set t2.Novelty=0 where t2.TweetID in (" + tweetIDsStr + ") and t2.Novelty > 0.6";
            Helpers.RunSqlStatement(Name, sql, false);
        }

        private static void UpdateNeighbor(Dictionary<long, TweetNeighbor> neighborInfo, long tweetID, long assignedClusterID)
        {
            if (!neighborInfo.ContainsKey(tweetID))
                neighborInfo.Add(tweetID, new TweetNeighbor() { TweetID = tweetID, TweetClusterID = assignedClusterID, Degree = 0 });
            else
                neighborInfo[tweetID].TweetClusterID = assignedClusterID;
        }

        void MarkTweetsAsProcessed(IEnumerable<LSHashTweet> tweets)
        {
            if (!tweets.Any())
                return;

            string tweetIDs = String.Join(",", tweets.Select(n => n.ID.ToString()).ToArray());

            Helpers.RunSqlStatement(Name, "update Tweet set CalculatedRelations = 1 where TweetID in (" + tweetIDs + ");");
        }

        LSHashFunction GetNewHashFunction()
        {
            //Generate hyper planes
            List<WordVector> planes = new List<WordVector>();
            for (int j = 0; j < Settings.TweetClusterer_TCW_HyperPlaneCount; j++)
            {
                WordVector plane = new WordVector();
                List<long> wordIDs = GetRandomWordIDs(Settings.TweetClusterer_TCW_WordsPerHyperPlane);
                foreach (long id in wordIDs)
                    plane.AddItem(id, Helpers.NextGaussian());
                planes.Add(plane);
                Console.Write('.');
            }
            return new LSHashFunction(planes);
        }

        public void UpdateOldestHashFunction()
        {
            //Create a hash function and hash table
            LSHashFunction hashFunc = GetNewHashFunction();
            _tables[_nextTableRehashIndex].SetNewHashFunction(hashFunc);

            _nextTableRehashIndex = (_nextTableRehashIndex + 1) % Settings.TweetClusterer_TCW_HashTableCount;
        }

        public void CleanDeletedOrArchivedTweets()
        {
            //Get all tweet IDs stored in the hash tables
            HashSet<long> tweetIDs = new HashSet<long>();
            foreach (var table in _tables)
                tweetIDs.UnionWith(table.GetTweetIDs());
            
            if (tweetIDs.Count == 0)
                return;

            //Check which of these have been deleted from the database
            List<long> allTweetIDs = new List<long>(tweetIDs);
            HashSet<long> remainingTweetIDs = new HashSet<long>();
            int batchSize = 1000;
            for (int i = 0; i < (int)Math.Ceiling(allTweetIDs.Count / (double)batchSize); i++)
            {
                List<long> batch = allTweetIDs.GetRange(i * batchSize, Math.Min(batchSize, allTweetIDs.Count - i * batchSize));
                string idStr = string.Join(",", batch.Select(n => n.ToString()).ToArray());

                string sql = "select TweetID from Tweet natural join TweetCluster where TweetID in (" + idStr + ") and not IsArchived";
                Helpers.RunSelect(Name, sql, remainingTweetIDs, (nothing, reader) =>
                    remainingTweetIDs.Add(reader.GetInt64("TweetID"))
                );
            }
            List<long> deletedTweetIDs = allTweetIDs.Where(n => !remainingTweetIDs.Contains(n)).ToList();

            //Remove deleted tweets from the hash tables
            foreach (var table in _tables)
                table.RemoveTweetsByID(deletedTweetIDs);

            if (deletedTweetIDs.Count > 0)
                Console.WriteLine("Removed " + deletedTweetIDs.Count + " archived or deleted tweets from the hashtables");
        }

        public void CreateHashTables()
        {
            for (int i = 0; i < Settings.TweetClusterer_TCW_HashTableCount; i++)
            {
                Console.Write("Making new hash table");
                LSHashFunction hashFunc = GetNewHashFunction();
                _tables.Add(new LSHashTable(hashFunc, Settings.TweetClusterer_TCW_HashBinSize));
                Console.WriteLine(" done");
            }
        }

        List<long> GetRandomWordIDs(int n)
        {
            string sql = @"select WordID from WordScore
                where Score4d < (select Value from Constants where Name='WordScore4dHigh')
                and Score4d > 3
                and Score1h / Score4d > 0.005
                order by rand()
                limit " + n + ';';
            //BASELINE EDIT
//            string sql = @"select WordID from WordScore
//                where Score4d < (select Value from Constants where Name='WordScore4dHigh')
//                and Score4d > 3
//                order by rand()
//                limit " + n + ';';

            List<long> wordIDs = new List<long>();
            Helpers.RunSelect(Name, sql, wordIDs, (values, reader) => values.Add(Convert.ToInt64(reader[0])));
            return wordIDs;
        }

        public void InitializeWithOldTweets()
        {
            //Load lots of old tweets and process them without storing any relationships
            List<LSHashTweet> tweets = GetTweetBatch(Settings.TweetClusterer_TCW_InitializeSize, true);

            foreach (LSHashTweet tweet in tweets)
            {
                //Rough approximation of novely calculations in real processing loop
                foreach (LSHashTable table in _tables)
                {
                    bool dummy;
                    var neighbors = table.Add(tweet, out dummy);
                }

                _history.Add(tweet);
            }

            Console.WriteLine("Initialized with " + tweets.Count + " tweets");
        }
    }
}
