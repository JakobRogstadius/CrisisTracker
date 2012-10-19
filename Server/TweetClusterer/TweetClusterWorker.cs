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
        int _wordsPerHyperPlane = 1 + (int)(Math.Log(Math.Pow(Settings.TweetClusterer_TCW_DictionaryWordCount, 2)));

        List<LSHashTable> _tables = new List<LSHashTable>();
        LSHashTweetHistory _history = new LSHashTweetHistory(Settings.TweetClusterer_TCW_HistorySize);
        Random _random = new Random();

        int _nextTableRehashIndex = 0;

        public int ProcessTweetBatch()
        {
            try
            {
                List<LSHashTweet> tweets = GetTweetBatch(Settings.TweetClusterer_TCW_BatchSize, getProcessed: false);
                Console.WriteLine("Fetched " + tweets.Count + " tweets");
                if (tweets.Count == 0)
                    return 0;

                List<TweetRelationSimple> relations = new List<TweetRelationSimple>();

                //Process each tweet
                foreach (LSHashTweet t in tweets)
                {
                    //Skip content-less tweets
                    if (t.Vector.ItemCount < Settings.TweetClusterer_TCW_MinTweetWordCount
                        || t.Vector.OriginalLength < Settings.TweetClusterer_TCW_MinTweetVectorLength)
                    {
                        continue;
                    }

                    //Add the tweet to each hash table and keep track of collisions with previously added tweets
                    Dictionary<LSHashTweet, int> neighborCandidates = new Dictionary<LSHashTweet, int>();
                    foreach (LSHashTable table in _tables)
                    {
                        List<LSHashTweet> hits = table.Add(t);
                        foreach (LSHashTweet hit in hits)
                        {
                            if (neighborCandidates.ContainsKey(hit))
                                neighborCandidates[hit]++;
                            else
                                neighborCandidates.Add(hit, 1);
                        }
                    }

                    //Look for relations in hash bin collisions
                    double maxSimilarity = 0;
                    int foundRelations = 0;
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
                            if (similarity > Settings.TweetClusterer_TCW_MinTweetSimilarityForLink && t.ID != candidate.ID)
                            {
                                relations.Add(new TweetRelationSimple() { TweetID = t.ID, TweetID2 = candidate.ID, Similarity = similarity });
                                foundRelations++;

                                if (similarity > maxSimilarity)
                                    maxSimilarity = similarity;
                            }
                        }
                    }

                    //If retweet, add relation to parent
                    if (t.RetweetOf.HasValue && !relations.Any(n => n.TweetID2 == t.RetweetOf.Value))
                        relations.Add(new TweetRelationSimple() { TweetID = t.ID, TweetID2 = t.RetweetOf.Value, Similarity = 0.99 });

                    //If no neighbor from bins, search in history
                    if (foundRelations < 2)
                    {
                        List<LSHashTweet> nearestCandidates = _history.GetNearestNeighbors(t, Settings.TweetClusterer_TCW_MaxLinksPerTweet - foundRelations);
                        foreach (LSHashTweet candidate in nearestCandidates)
                        {
                            double similarity = candidate.Vector * t.Vector;
                            if (similarity > Settings.TweetClusterer_TCW_MinTweetSimilarityForLink)
                            {
                                relations.Add(new TweetRelationSimple() { TweetID = t.ID, TweetID2 = candidate.ID, Similarity = similarity });
                                foundRelations++;

                                if (similarity > maxSimilarity)
                                    maxSimilarity = similarity;
                            }
                        }
                    }

                    if (maxSimilarity < Settings.TweetClusterer_TCW_IdentityThreshold)
                        _history.Add(t); //Only add tweets that were "unique" or "novel" according to the hash tables
                }

                Console.WriteLine("Total items in all hash tables: " + _tables.Sum(n => n.GetItemCount()));

                StoreRelations(relations);
                Console.WriteLine("Stored relations... ");

                AssignTweetClusterIDsToBatchTweets(tweets, relations);
                Console.WriteLine();
                Console.Write("assigned cluster IDs... ");
                
                MarkTweetsAsProcessed(tweets);
                Console.WriteLine("and marked as processed.");

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
                sqlSB.Append(relation.Similarity);
                sqlSB.Append(')');
            }
            sqlSB.Append(';');
            Helpers.RunSqlStatement(Name, sqlSB.ToString(), false);
            Helpers.RunSqlStatement(Name, "insert ignore into TweetRelation select b.* from TweetRelationInsertBuffer b join Tweet t1 on t1.TweetID=b.TweetID1 join Tweet t2 on t2.TweetID=b.TweetID2;", false);
            Helpers.RunSqlStatement(Name, "delete from TweetRelationInsertBuffer;", false);
        }

        private void AssignTweetClusterIDsToBatchTweets(List<LSHashTweet> tweets, List<TweetRelationSimple> relations)
        {
            if (tweets.Count == 0)
                return;

            //Update cluster IDs for tweets in batch

            //Get the next unused cluster ID from the database
            long? nextClusterID = 0;
            Helpers.RunSelect(Name,
                "select coalesce(max(TweetClusterID)+1,0) as ID from TweetCluster;",
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
                group by T.TweetID;",
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

            HashSet<long> affectedTweetClusterIDs = new HashSet<long>(neighborInfo.Where(n => n.Value.TweetClusterID.HasValue).Select(n => n.Value.TweetClusterID.Value));

            //Group the edges by TweetID1 (tweet in batch)
            var tweetsWithRelations =
                from edge in relations
                group edge by edge.TweetID into tweetEdges
                select new { TweetID = tweetEdges.Key, Edges = tweetEdges };
            

            //Assign cluster ID to each tweet in the batch: the majority ID in the local neighborhood if available, else the next unused ID
            List<long> createdClusterIDs = new List<long>();
            Dictionary<long, long> assignedTweetClusterIDs = new Dictionary<long, long>(); //tweetID, clusterID
            foreach (var tweet in tweetsWithRelations)
            {
                var neighbors = tweet.Edges
                    .Where(e => neighborInfo.ContainsKey(e.TweetID2))
                    .Select(e => neighborInfo[e.TweetID2])
                    .ToList();
                if (!neighbors.Any()) //I think this happens when edges were added for retweets, but the source tweet was never seen
                    continue;

                var idGroups =
                    from n in neighbors
                    group n by n.TweetClusterID into g
                    select new { ID = g.Key, Members = g, Score = g.Sum(m => 1 + 0.3*Math.Log(m.Degree)) };
                long? topGroup = idGroups.OrderByDescending(n => n.Score).First().ID;

                if (topGroup.HasValue)
                    assignedTweetClusterIDs.Add(tweet.TweetID, topGroup.Value);
                else
                {
                    assignedTweetClusterIDs.Add(tweet.TweetID, nextClusterID.Value);
                    createdClusterIDs.Add(nextClusterID.Value);
                    nextClusterID++;
                }

                //Update neighbor info index with new assignment
                if (neighborInfo.ContainsKey(tweet.TweetID))
                    neighborInfo[tweet.TweetID].TweetClusterID = assignedTweetClusterIDs[tweet.TweetID];
            }
            
            //Assign cluster IDs to those tweets for which no relations were found
            foreach (LSHashTweet tweet in tweets)
            {
                if (assignedTweetClusterIDs.ContainsKey(tweet.ID))
                    continue;

                assignedTweetClusterIDs.Add(tweet.ID, nextClusterID.Value);
                createdClusterIDs.Add(nextClusterID.Value);
                nextClusterID++;
            }


            Console.Write(".");
            //Insert new TweetClusters (otherwise the tweet update below will fail from trigger constraints)
            string tweetClusterInsert = "INSERT IGNORE INTO TweetCluster (TweetClusterID, StartTime, EndTime) VALUES (" +
                string.Join(",'3000-01-01',0),(", assignedTweetClusterIDs.Values.Distinct().Select(n => n.ToString()).ToArray()) + ",'3000-01-01',0);";
            Helpers.RunSqlStatement(Name, tweetClusterInsert, false);
            Console.Write(".");


            //Save assignments in DB
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO Tweet (TweetID, TweetClusterID) VALUES ");
            bool first = true;
            foreach (var item in assignedTweetClusterIDs)
            {
                if (first) first = false;
                else sb.AppendLine(",");
                sb.Append("(" + item.Key + "," + item.Value + ")");
            }
            sb.AppendLine();
            sb.AppendLine("ON DUPLICATE KEY UPDATE TweetClusterID=VALUES(TweetClusterID);");
            Console.Write(".");

            Helpers.RunSqlStatement(Name, sb.ToString(), false);

            //Update EndTime of affected TweetClusters, to catch those cases where a tweet is assigned by the hash tables to an archived TweetCluster
            string sqlTweetCluster = @"
                insert into TweetCluster (TweetClusterID, EndTime)
                select
                    T.TweetClusterID,
                    t2.CreatedAt as EndTime
                from (
                    select 
                        TweetClusterID, 
                        max(TweetID) as MaxTweetID
                    from Tweet
                    where TweetID in (" + batchIDsStr + @") 
                    group by TweetClusterID
                ) T
                join Tweet t2 on t2.TweetID = T.MaxTweetID
                on duplicate key update
                    EndTime=greatest(EndTime,VALUES(EndTime))
                ;";
            Helpers.RunSqlStatement(Name, sqlTweetCluster, false);
            Console.Write(".");


            //Update all TweetCluster counts that may have changed, from recent additions or by moving out of the 4h time window
            //Note: The where clause looks silly, but it makes better use of indexes than if the first () is removed
            string sqlTweetClusterCounts = @"
                insert into TweetCluster (TweetClusterID, TweetCount, TweetCountRecent, RetweetCount, RetweetCountRecent, UserCount, UserCountRecent, TopUserCount, TopUserCountRecent, StartTime, EndTime)
                select TweetClusterID,
                    sum(RetweetOf is null) as TweetCount,
                    sum(RetweetOf is null and CreatedAt > T4h) as TweetCountRecent,
                    sum(RetweetOf is not null) as RetweetCount,
                    sum(RetweetOf is not null and CreatedAt > T4h) as RetweetCountRecent,
                    count(distinct if(IsBlacklisted, null, UserID)) as UserCount,
                    count(distinct if(CreatedAt > T4h, UserID, null)) as UserCountRecent,
                    count(distinct if(IsTopUser and not IsBlacklisted, UserID, null)) as TopUserCount,
                    count(distinct if(IsTopUser and not IsBlacklisted and CreatedAt > T4h, UserID, null)) as TopUserCountRecent,
                    min(CreatedAt) as StartTime,
                    max(CreatedAt) as EndTime
                from
                    TweetCluster
                    natural join Tweet
                    natural join TwitterUser,
                    (select max(CreatedAt) - interval 4 hour as T4h, max(CreatedAt) - interval 10 minute as T10m from Tweet where CalculatedRelations) as TTT
                where 
                    (UserCountRecent > 0 or EndTime > T10m or StartTime > T10m) 
                    and ((UserCountRecent > 0 and StartTime < T4h) or EndTime > T10m or StartTime > T10m)
                group by TweetClusterID
                on duplicate key update
                    TweetCount=VALUES(TweetCount),
                    TweetCountRecent=VALUES(TweetCountRecent),
                    RetweetCount=VALUES(RetweetCount),
                    RetweetCountRecent=VALUES(RetweetCountRecent),
                    UserCount=VALUES(UserCount),
                    UserCountRecent=VALUES(UserCountRecent),
                    TopUserCount=VALUES(TopUserCount),
                    TopUserCountRecent=VALUES(TopUserCountRecent),
                    StartTime=VALUES(StartTime),
                    EndTime=VALUES(EndTime)
                ;";
            Helpers.RunSqlStatement(Name, sqlTweetClusterCounts, false);
            Console.Write(".");

            //Update TweetCluster titles
            string sqlTweetClusterTitles = @"
                update TweetCluster tc
                set Title = 
                    (select IF(Text REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2), Text)
                    from Tweet where Tweet.TweetClusterID = tc.TweetClusterID order by length(Text) desc limit 1)
                where (Title = '' or Title is null)
                    and EndTime > (select max(CreatedAt) - interval 10 minute from Tweet where CalculatedRelations)
                ;";
            Helpers.RunSqlStatement(Name, sqlTweetClusterTitles, false);
            Console.Write(".");
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
                List<long> wordIDs = GetRandomWordIDs(_wordsPerHyperPlane);
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

        public void CleanDeletedTweets()
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

                string sql = "select TweetID from Tweet where TweetID in (" + idStr + ");";
                Helpers.RunSelect(Name, sql, remainingTweetIDs, (nothing, reader) =>
                    remainingTweetIDs.Add(reader.GetInt64("TweetID"))
                );
            }
            List<long> deletedTweetIDs = allTweetIDs.Where(n => !remainingTweetIDs.Contains(n)).ToList();

            //Remove deleted tweets from the hash tables
            foreach (var table in _tables)
                table.RemoveTweetsByID(deletedTweetIDs);

            Console.WriteLine("Removed " + deletedTweetIDs.Count + " tweets from the database, which no longer exist in the database");
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
            //Load 5000 old tweets and add process them without storing any relationships
            List<LSHashTweet> tweets = GetTweetBatch(Settings.TweetClusterer_TCW_InitializeSize, true);

            foreach (LSHashTweet tweet in tweets)
            {
                //Rough approximation of novely calculations in real processing loop
                foreach (LSHashTable table in _tables)
                {
                    var neighbors = table.Add(tweet);
                }

                _history.Add(tweet);
            }

            Console.WriteLine("Initialized with " + tweets.Count + " tweets");
        }
    }
}
