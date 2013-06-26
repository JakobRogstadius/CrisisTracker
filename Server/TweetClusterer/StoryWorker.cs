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
using Procurios.Public;
using System.Collections;
using MySql.Data.MySqlClient;

namespace CrisisTracker.TweetClusterer
{
    public class StoryWorker
    {
        class MergeAction
        {
            public long KeptStoryID { get; set; }
            public long MergedStoryID { get; set; }
        }

        class SplitAction
        {
            public long StoryID { get; set; }
            public long TweetClusterID { get; set; }
        }

        class SimpleTweetCluster
        {
            public long TweetClusterID { get; set; }
            public long? StoryID { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public WordVector WordVector { get; set; }
            public bool isNewStory { get; set; }

            public SimpleTweetCluster()
            {
                WordVector = new WordVector();
            }

            public override bool Equals(object obj)
            {
                if (obj is SimpleTweetCluster)
                    return TweetClusterID.Equals(((SimpleTweetCluster)obj).TweetClusterID);
                return false;
            }

            public override int GetHashCode()
            {
                return TweetClusterID.GetHashCode();
            }
        }

        class SimpleStory
        {
            public long StoryID { get; set; }
            public WordVector WordVector { get; set; }
            public HashSet<string> Tags { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public SimpleStory()
            {
                WordVector = new WordVector();
            }

            public bool IsSimilarTo(SimpleStory story)
            {
                return Similarity(story) >= 0.5;
            }

            public double Similarity(SimpleStory story)
            {
                double wordSim = WordVector * story.WordVector;
                
                double tagCount = Tags.Union(story.Tags).Count();
                double commonCount = Tags.Intersect(story.Tags).Count();
                double commonRatio = commonCount / Tags.Count();
                double differentRatio = (tagCount - commonCount) / tagCount;
                double topicCommonality = Math.Max(-1, Math.Min(1, (commonRatio - differentRatio) * 0.2 * Math.Min(5, tagCount)));
                
                int sameTime = StartTime > story.StartTime && StartTime < story.EndTime.AddMinutes(30)
                    || story.StartTime > StartTime && story.StartTime < EndTime.AddMinutes(30) ? 1 : 0;
                //int sameTopic = wordSim > 0.85 || wordSim > 0.5 && topicCommonality > 0.25 ? 1 : 0;
                //int similarTopic = wordSim > 0.4 || topicCommonality > 0.25 ? 1 : 0;
                //int sameLocation = 0;
                //int sharedUrl = 0;
                //int samePeople = 0;
                //int sameCount = sameTopic + samePeople + sameLocation + sameTime;

                ////Weighting scheme based on study of human-rated semantic similarity of tweets
                //double similarity = Math.Min(1, Math.Max(0, 
                //    0.3 * (sameCount==1 ? 1 : 0) 
                //    + 0.5 * (sameCount==2 ? 1 : 0)
                //    + 0.8 * (sameCount == 3 ? 1 : 0)
                //    + 1.0 * (sameCount == 4 ? 1 : 0)
                //    + 0.1 * sharedUrl 
                //    - 0.2 * (1 - similarTopic) 
                //    - 0.15 * sameLocation
                //    - 0.1 * samePeople
                //    - 0.2 * sameTime
                //    + 0.5));

                double similarity = (
                    0.85 * wordSim
                    + 0.4 * topicCommonality
                    ) * sameTime;

                return similarity;
            }
        }

        const string Name = "StoryWorker";

        public static bool Run()
        {
            try
            {
                Console.WriteLine("Running StoryWorker");

                //Apply user-initiated split and merge actions
                Console.WriteLine("Processing user-splits and merges...");
                ApplyPendingStorySplits();
                ApplyPendingStoryMerges();

                //Process new clusters and assign them to existing stories or create new stories
                var topStories = GetCurrentTopStories(Settings.TweetClusterer_SW_TopStoryCount);
                Console.WriteLine("Fetched top stories: " + topStories.Count);
                Console.WriteLine("Processing new clusters...");
                var unassignedClusters = GetUnassignedTweetClusters();
                Console.WriteLine("  unassigned clusters: " + unassignedClusters.Count);
                if (unassignedClusters.Count > 0)
                {
                    long nextStoryID = GetNextStoryID();
                    long nextStoryIDBefore = nextStoryID;
                    AssignStoryIDToNewClusters(ref nextStoryID, unassignedClusters, topStories);
                    Console.WriteLine("Clusters->new stories: " + (nextStoryID - nextStoryIDBefore));
                    Console.WriteLine("Clusters->top stories: " + (unassignedClusters.Count - (nextStoryID - nextStoryIDBefore)));
                    AssignStoryIDsToClustersInDB(unassignedClusters.Values);
                }

                //Check if any of the top stories should be merged
                Console.WriteLine("Merging stories...");
                List<MergeAction> mergeActions = FindDuplicateStories(topStories);
                Console.WriteLine("  top story merges: " + mergeActions.Count);
                
                //Process colliding clusters and merge stories where sufficiently similar
                var mergeCandidatePairs = GetMergeCandidateStoryPairs();
                Console.WriteLine("  merge candidates: " + mergeCandidatePairs.Count);
                if (mergeCandidatePairs.Count > 0)
                {
                    var mergeCandidateStories = GetStoriesByID(
                        mergeCandidatePairs.Select(n => n.KeptStoryID)
                        .Union(mergeCandidatePairs.Select(n => n.MergedStoryID))
                        .Distinct());
                    Console.WriteLine("  unique stories:  " + mergeCandidateStories.Count);
                    var mergeActions2 = EvaluateMergeCandidates(mergeCandidatePairs, mergeCandidateStories);
                    Console.WriteLine("  actual merges:   " + mergeActions2.Count);
                    mergeActions.AddRange(mergeActions2);
                }

                if (mergeActions.Count > 0)
                    MergeStories(mergeActions, isAutomerge: true);

                //Recompute summary stats for updated stories
                Console.WriteLine("Updating story stats...");
                UpateStoriesWithPendingChanges();
                ProcessTweetMetatags();

                MarkStoriesAsProcessed();

                //Delete processed candidate records from DB
                Console.WriteLine("Performing maintenance...");
                DoDBMaintenance();

                Console.WriteLine("StoryWorker complete!");

                return unassignedClusters.Count > 0;
            }
            catch (Exception e)
            {
                Output.Print(Name, e);
                throw e;
            }
        }

        static long GetNextStoryID()
        {
            long? nextStoryID = 0;
            Helpers.RunSelect(Name,
                "select coalesce(max(StoryID)+1,0) as ID from Story;",
                nextStoryID,
                (dummy, reader) => nextStoryID = Convert.ToInt64(reader["ID"]));
            return nextStoryID.Value;
        }

        static Dictionary<long, SimpleTweetCluster> GetUnassignedTweetClusters()
        {
            //Get word vector for each tweet cluster
            string query = @"
                select TweetClusterID, WordID, Weight from (
                select
                    TT.*,
                    @r := IF(@g=TweetClusterID,@r+1,1) RowNum,
                    @g := TweetClusterID
                from 
                    (select @g:=null) initvars
                    cross join
                    (
                        select
                            TweetClusterID,
                            WordID, 
                            count(*) * ScoreToIdf(Score4d) as Weight
                        from (select TweetClusterID from TweetCluster where StoryID is null order by 1) as TweetCluster
                            natural join Tweet 
                            natural join WordTweet 
                            natural join WordScore
                        group by TweetClusterID, WordID
                        order by TweetClusterID, Weight desc
                    ) TT
                ) TTT
                where RowNum <= " + Settings.TweetClusterer_SW_MaxWordsInStoryVector + @"
                limit 1000000;";

            //Parse returned word weights and build TweetCluster vectors
            Dictionary<long, SimpleTweetCluster> clusters = new Dictionary<long, SimpleTweetCluster>();
            Helpers.RunSelect(Name, query, clusters, (values, reader) =>
            {
                long clusterID = reader.GetInt64("TweetClusterID");
                if (!clusters.ContainsKey(clusterID))
                    clusters.Add(clusterID, new SimpleTweetCluster() { TweetClusterID = clusterID });

                values[reader.GetInt64("TweetClusterID")].WordVector.AddItem(
                    reader.GetInt64("WordID"),
                    reader.GetDouble("Weight"));
            });

            //Normalize vectors
            foreach (SimpleTweetCluster cluster in clusters.Values)
                cluster.WordVector.Normalize();

            return clusters;
        }

        static Dictionary<long, SimpleStory> GetCurrentTopStories(int storyCount)
        {
            //Get word vectors from top N stories
            string query = @"
                select 
	                StoryID, StartTime, EndTime, WordID, Weight
                from
	                (select StoryID, StartTime, EndTime from Story where not IsArchived order by WeightedSizeRecent desc limit " + storyCount + @") s
	                natural join StoryInfoKeywordTag
	                natural join InfoKeyword
	                join Word on Word.Word=InfoKeyword.Keyword
                ";

            //Parse returned word weights and build story vectors
            Dictionary<long, SimpleStory> stories = new Dictionary<long, SimpleStory>();
            Helpers.RunSelect(Name, query, stories, (values, reader) =>
            {
                long storyID = reader.GetInt64("StoryID");
                if (!stories.ContainsKey(storyID))
                {
                    stories.Add(storyID, new SimpleStory()
                    {
                        StoryID = storyID,
                        StartTime = reader.GetDateTime("StartTime"),
                        EndTime = reader.GetDateTime("EndTime")
                    });
                }

                stories[storyID].WordVector.AddItem(
                    reader.GetInt64("WordID"),
                    reader.GetDouble("Weight"));
            });

            //Normalize vectors
            foreach (SimpleStory story in stories.Values)
                story.WordVector.Normalize();

            return stories;
        }

        static void AssignStoryIDToNewClusters(ref long nextStoryID, Dictionary<long, SimpleTweetCluster> clusters, Dictionary<long, SimpleStory> stories)
        {
            if (!clusters.Any())
                return;

            foreach (SimpleTweetCluster cluster in clusters.Values)
            {
                //Get the distribution of similarity scores between this cluster and each of the most active stories
                var similarities = stories.Values
                    .Select(n => new { StoryID = n.StoryID, Similarity = n.WordVector * cluster.WordVector })
                    .OrderByDescending(n => n.Similarity)
                    .ToList();

                //Check if there is a rapid drop somewhere in the similarity distribution
                bool distrHasRapidDrop = false;
                if (similarities.Count > 1)
                {
                    for (int i = 1; i < similarities.Count; i++)
                    {
                        if (similarities[i].Similarity > 0.01 && similarities[i].Similarity < Settings.TweetClusterer_SW_MergeDropScale * similarities[i - 1].Similarity)
                        {
                            distrHasRapidDrop = true;
                            break;
                        }
                        if (similarities[i].Similarity < Settings.TweetClusterer_SW_MergeThresholdWithDrop)
                            break;
                    }
                }

                //Assign a story ID to the cluster
                if (stories.Count > 0
                    && (similarities[0].Similarity > Settings.TweetClusterer_SW_MergeThreshold
                        || similarities[0].Similarity > Settings.TweetClusterer_SW_MergeThresholdWithDrop && distrHasRapidDrop))
                {
                    cluster.StoryID = similarities[0].StoryID;
                }
                else
                {
                    cluster.StoryID = nextStoryID;
                    cluster.isNewStory = true;
                    nextStoryID++;
                }
            }
        }

        static List<MergeAction> FindDuplicateStories(Dictionary<long, SimpleStory> stories)
        {
            List<MergeAction> mergeActions = new List<MergeAction>();

            foreach (SimpleStory story in stories.OrderBy(n => n.Key).Select(n => n.Value))
            {
                var newerStories = stories
                    .Where(n => n.Key > story.StoryID).OrderBy(n => n.Key).Select(n => n.Value);

                if (!newerStories.Any())
                    break;

                //Get the distribution of similarity scores between this cluster and each of the most active stories
                var similarities = newerStories
                    .Select(n => new { StoryID = n.StoryID, Similarity = n.WordVector * story.WordVector })
                    .OrderByDescending(n => n.Similarity)
                    .ToList();

                //Check if there is a rapid drop somewhere in the similarity distribution
                bool distrHasRapidDrop = false;
                if (similarities.Count > 1)
                {
                    for (int i = 1; i < similarities.Count; i++)
                    {
                        if (similarities[i].Similarity > 0.01 
                            && similarities[i].Similarity < Settings.TweetClusterer_SW_MergeDropScale * similarities[i - 1].Similarity)
                        {
                            distrHasRapidDrop = true;
                            break;
                        }
                    }
                }

                //Merge the stories if similar
                if ((similarities[0].Similarity > Settings.TweetClusterer_SW_MergeThreshold
                    || similarities[0].Similarity > Settings.TweetClusterer_SW_MergeThresholdWithDrop && distrHasRapidDrop))
                {
                    long targetStoryID = story.StoryID;
                    var thisStoryMerges = mergeActions.Where(n => n.MergedStoryID == story.StoryID);
                    if (thisStoryMerges.Any())
                        targetStoryID = thisStoryMerges.Min(n => n.KeptStoryID);
                    mergeActions.Add(new MergeAction() { KeptStoryID = targetStoryID, MergedStoryID = similarities[0].StoryID });
                }
            }

            return mergeActions;
        }

        static List<MergeAction> GetMergeCandidateStoryPairs()
        {
            //Get word vector for each cluster
            string query = 
                @"select distinct
	                tc1.StoryID as StoryID1,
	                tc2.StoryID as StoryID2
                from TweetClusterCollision tcc
                join TweetCluster tc1 on tc1.TweetClusterID=tcc.TweetClusterID1
                join TweetCluster tc2 on tc2.TweetClusterID=tcc.TweetClusterID2
                where tc1.StoryID!=tc2.StoryID";

            List<MergeAction> collisions = new List<MergeAction>();
            Helpers.RunSelect(Name, query, collisions, (values, reader) =>
            {
                collisions.Add(new MergeAction()
                {
                    KeptStoryID = reader.GetInt64("TweetClusterID1"),
                    MergedStoryID = reader.GetInt64("TweetClusterID1")
                });
            });

            return collisions;
        }

        //TODO: Also get non-keyword tags for stories
        static Dictionary<long, SimpleStory> GetStoriesByID(IEnumerable<long> storyIDs)
        {
            if (!storyIDs.Any())
                return new Dictionary<long, SimpleStory>();

            //Get word vectors from top N stories
            string query = @"
                select 
	                StoryID, StartTime, EndTime, WordID, Weight
                from
	                StoryID
	                natural join StoryInfoKeywordTag
	                natural join InfoKeyword
                where StoryID in (" + String.Join(",", storyIDs.Select(n => n.ToString()).ToArray()) + @")
                ";

            //Parse returned word weights and build story vectors
            Dictionary<long, SimpleStory> stories = new Dictionary<long, SimpleStory>();
            Helpers.RunSelect(Name, query, stories, (values, reader) =>
            {
                long storyID = reader.GetInt64("StoryID");
                if (!stories.ContainsKey(storyID))
                {
                    stories.Add(storyID, new SimpleStory()
                    {
                        StoryID = storyID,
                        StartTime = reader.GetDateTime("StartTime"),
                        EndTime = reader.GetDateTime("EndTime")
                    });
                }

                stories[storyID].WordVector.AddItem(
                    reader.GetInt64("WordID"),
                    reader.GetDouble("Weight"));
            });

            //Normalize vectors
            foreach (SimpleStory story in stories.Values)
                story.WordVector.Normalize();

            return stories;
        }

        static List<MergeAction> EvaluateMergeCandidates(List<MergeAction> candidates, Dictionary<long, SimpleStory> stories)
        {
            List<MergeAction> mergeActions = new List<MergeAction>();

            foreach (var storyPair in candidates)
            {
                SimpleStory s1 = stories[storyPair.KeptStoryID];
                SimpleStory s2 = stories[storyPair.MergedStoryID];

                if (s1.IsSimilarTo(s2))
                    mergeActions.Add(storyPair);
            }

            return mergeActions;
        }

        static void AssignStoryIDsToClustersInDB(ICollection<SimpleTweetCluster> clusters)
        {
            if (!clusters.Any())
                return;

            //Insert new Stories (otherwise the TweetCluster update below will fail from trigger constraints)
            string sql = 
                "insert ignore into Story (StoryID, StartTime, EndTime, PendingUpdate) values "
                + String.Join(",", clusters.Select(n => 
                    "(" 
                    + n.StoryID + ",'" 
                    + n.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + "','"
                    + n.EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "',"
                    + "1)").ToArray())
                + " on duplicate key update PendingUpdate=1";
            Helpers.RunSqlStatement(Name, sql, false);

            //Assign StoryID to clusters
            sql = "insert into TweetCluster (TweetClusterID, StoryID, PendingStoryUpdate) values "
                + String.Join(",", clusters.Select(n => "(" + n.TweetClusterID + "," + n.StoryID + ", 1)").ToArray())
                + " on duplicate key update StoryID=values(StoryID), PendingStoryUpdate=1";
            Helpers.RunSqlStatement(Name, sql, false);
        }

        static void MergeStories(IEnumerable<MergeAction> storyMerges, bool isAutomerge)
        {
            //Update StoryID of clusters in stories that were merged
            foreach (MergeAction merge in storyMerges)
            {
                //Move content and meta-data from merged story to target story, then delete the merged story
                string sql =
                    @"update TweetCluster set PendingStoryUpdate=1, StoryID=" + merge.KeptStoryID + " where StoryID=" + merge.MergedStoryID + @"; 
                    insert into StoryInfoKeywordTag (StoryID, InfoKeywordID, CreatedAt, UserID, Weight) 
                        select " + merge.KeptStoryID + ", t2.InfoKeywordID, t2.CreatedAt, t2.UserID, t2.Weight from StoryInfoKeywordTag t2 where t2.StoryID=" + merge.MergedStoryID + @"
                        on duplicate key update StoryInfoKeywordTag.Weight=StoryInfoKeywordTag.Weight+values(Weight);
                    delete from StoryInfoKeywordTag where StoryID=" + merge.MergedStoryID + @";
                    insert into StoryAidrAttributeTag (StoryID, AttributeID, LabelID, TagCount) 
                        select " + merge.KeptStoryID + ", t.AttributeID, t.LabelID, t.TagCount from StoryAidrAttributeTag t where StoryID=" + merge.MergedStoryID + @"
                        on duplicate key update StoryAidrAttributeTag.TagCount=StoryAidrAttributeTag.TagCount+values(TagCount);
                    delete from StoryAidrAttributeTag where StoryID=" + merge.MergedStoryID + @";
                    update ignore StoryInfoCategoryTag set StoryID=" + merge.KeptStoryID + " where StoryID=" + merge.MergedStoryID + @"; 
                    update ignore StoryInfoEntityTag set StoryID=" + merge.KeptStoryID + " where StoryID=" + merge.MergedStoryID + @"; 
                    update ignore StoryLocationTag set StoryID=" + merge.KeptStoryID + " where StoryID=" + merge.MergedStoryID + @";
                    update ignore StoryCustomTitle set StoryID=" + merge.KeptStoryID + " where StoryID=" + merge.MergedStoryID + @";
                    insert into StoryMerges (StoryID1, StoryID2, MergedAt, IP, UserID) values (" + merge.KeptStoryID + "," + merge.MergedStoryID + @",utc_timestamp(),0,0) 
                        on duplicate key update MergedAt=values(MergedAt);
                    delete from Story where StoryID=" + merge.MergedStoryID + ";";
                Helpers.RunSqlStatement(Name, sql, false);

                if (isAutomerge)
                {
                    //Increment story merge counter
                    string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:00:00");
                    sql = 
                        "insert into HourStatistics (DateHour, StoriesAutoMerged) values ('" + now + @"', 1) 
                        on duplicate key update StoriesAutoMerged=StoriesAutoMerged+1;";
                    Helpers.RunSqlStatement(Name, sql, false);
                }
            }
        }

        static void SplitStories(IEnumerable<SplitAction> storySplits)
        {
            if (!storySplits.Any())
                return;

            string sql = "update TweetCluster set StoryID=null where TweetClusterID in (" +
                string.Join(",", storySplits.Select(n => n.TweetClusterID.ToString()).ToArray()) + ")";
            Helpers.RunSqlStatement(Name, sql, false);

            sql = "update Story set PendingUpdate=1 where StoryID in (" +
                string.Join(",", storySplits.Select(n => n.StoryID.ToString()).ToArray()) + ")";
            Helpers.RunSqlStatement(Name, sql, false);
        }

        static void UpateStoriesWithPendingChanges()
        {
            //Allow dirty reads
            Helpers.RunSqlStatement(Name, "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", false);
            
            //Update all Story counts that may have changed, from recent additions or by moving out of the 4h time window
            Helpers.RunSqlStatement(Name, 
                "update Story natural join (select distinct StoryID from TweetCluster where PendingStoryUpdate) T set PendingUpdate=1", false);            
            
            string sql = @"
                insert into Story (StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, TopUserCountRecent, StartTime, EndTime, WeightedSize, WeightedSizeRecent)
                select StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, TopUserCountRecent, StartTime, EndTime,
                    least(UserCount, TweetCount + 0.5*log(10+RetweetCount)) as WeightedSize,
                    least(UserCountRecent, TweetCountRecent + 0.5*log(10+RetweetCountRecent)) as WeightedSizeRecent
                from (
	                select
		                s.StoryID,
		                sum(RetweetOf is null) as TweetCount,
		                sum(RetweetOf is null and CreatedAt > T4h) as TweetCountRecent,
		                sum(RetweetOf is not null) as RetweetCount,
		                sum(RetweetOf is not null and CreatedAt > T4h) as RetweetCountRecent,
		                count(distinct t.UserID) as UserCount,
		                count(distinct if(CreatedAt > T4h, t.UserID, null)) as UserCountRecent,
		                count(distinct if(IsTopUser, t.UserID, null)) as TopUserCount,
		                count(distinct if(CreatedAt > T4h and IsTopUser, t.UserID, null)) as TopUserCountRecent,
                        
		                min(tc.StartTime) as StartTime,
		                max(tc.EndTime) as EndTime
	                from
		                Story s
		                join TweetCluster tc on tc.StoryID=s.StoryID
		                join Tweet t on t.TweetClusterID=tc.TweetClusterID
		                join TwitterUser tu on tu.UserID=t.UserID,
		                (select max(CreatedAt) - interval 4 hour as T4h from Tweet where CalculatedRelations) as TTT
                    where 
                        s.PendingUpdate
                        and not tu.IsBlacklisted
                        or s.StartTime < T4h and WeightedSizeRecent>0
                    group by s.StoryID
                    ) TT
                on duplicate key update
                    TweetCount=VALUES(TweetCount),
                    RetweetCount=VALUES(RetweetCount),
                    UserCount=VALUES(UserCount),
                    TopUserCount=VALUES(TopUserCount),
                    TopUserCountRecent=VALUES(TopUserCountRecent),
                    WeightedSize=VALUES(WeightedSize),
                    WeightedSizeRecent=VALUES(WeightedSizeRecent),
                    StartTime=VALUES(StartTime),
                    EndTime=VALUES(EndTime)
                ";
            Helpers.RunSqlStatement(Name, sql, false);

            //Update Story titles
            sql =
                @"insert into Story (StoryID, Title)
                select StoryID, tc.Title
                from TweetCluster tc
                natural join (
	                select s2.StoryID, max(tc2.TweetCount) as TweetCount
	                from TweetCluster tc2 join Story s2 on tc2.StoryID=s2.StoryID
	                where s2.PendingUpdate and not s2.IsArchived
	                group by s2.StoryID) T
                on duplicate key update Title=values(Title)";
            Helpers.RunSqlStatement(Name, sql, false);

            //Update trends
//            sql =
//                @"
//                insert into Story (StoryID, TweetTrend, RetweetTrend)
//                select zeros.StoryID, group_concat(coalesce(Tweets,0)) TweetTrend, group_concat(coalesce(Retweets,0)) RetweetTrend from (
//                    select StoryID, step from 
//                    (select StoryID from Story where PendingUpdate) s
//                    cross join (
//                        select 0 step  union select 1 union select 2 union select 3 union select 4 
//                        union select 5 union select 6 union select 7 union select 8 union select 9 
//                        union select 10 union select 11 union select 12 union select 13 union select 14 
//                        union select 15 union select 16 union select 17 union select 18 union select 19
//                    ) st
//                ) zeros left join (
//                    select
//                        s.StoryID,
//                        floor(19.99*(unix_timestamp(CreatedAt) - minT)/rangeT) as step,
//                        sum(RetweetOf is null) as Tweets,
//                        sum(RetweetOf is not null) as Retweets
//                    from
//                    (
//                        select 
//                            StoryID, 
//                            unix_timestamp(StartTime) minT,
//                            1+unix_timestamp(EndTime)-unix_timestamp(StartTime) rangeT
//                        from Story where PendingUpdate
//                    ) topS
//                    join Story s on s.StoryID=topS.StoryID
//                    join TweetCluster tc on tc.StoryID=s.StoryID
//                    join Tweet t on t.TweetClusterID=tc.TweetClusterID
//                    join TwitterUser u on u.UserID = t.UserID
//                    where not u.IsBlacklisted
//                    group by 1,2
//                ) vals on vals.StoryID=zeros.StoryID and vals.step=zeros.step
//                group by zeros.StoryID
//                on duplicate key update TweetTrend=values(TweetTrend), RetweetTrend=values(RetweetTrend)";
//            Helpers.RunSqlStatement(Name, sql, false);

            //Add keyword tags to stories based on top words in stories (based on up to N random tweets per story)
            int N = 200;
            sql = @"
                start transaction;
                select @g:=null, @r:=0, @s:=0, @c:=0;
                
                create temporary table __StoryKeywordTagsTmp as
                select StoryID, Word, Weight
                from (
	                select X.*,
		                @r := IF(@g=StoryID,@r+1,1) RowNum,
		                @s := IF(@g=StoryID,@s+Weight,Weight) AccuScore,
		                @c := IF(@g=StoryID,@c+Occurences,Occurences) AccuOcc,
		                @g := StoryID
	                from (
		                select
			                s.StoryID, 
			                Word,
			                count(*) as Occurences,
			                count(*) * ScoreToIdf(Score4d) as Weight,
			                if(Word like '#%' or (select 1 from TwitterTrackFilter where FilterType = 0 and Word = Word.Word and IsActive limit 1), 1, 0) as TopicWord
		                from
			                Story s
			                join TweetCluster tc on tc.StoryID=s.StoryID
			                join Tweet t on t.TweetClusterID=tc.TweetClusterID
			                natural join WordTweet
			                natural join Word
			                natural join WordScore
		                where not s.IsArchived
			                and (s.PendingUpdate or tc.PendingStoryUpdate)
			                and mod(t.TweetID,100) < 100*(" + N + @" / (s.TweetCount+s.RetweetCount))
		                group by s.StoryID, WordID
	                ) X
	                order by StoryID,Weight desc
                ) X
                where Weight/AccuScore>0.04 or (TopicWord and Occurences/AccuOcc > 0.04);

                insert ignore into InfoKeyword (Keyword) select Word from __StoryKeywordTagsTmp;

                insert into StoryInfoKeywordTag (StoryID, InfoKeywordID, Weight, CreatedAt, UserID) 
                select StoryID, InfoKeywordID, Weight, utc_timestamp(), 0 from __StoryKeywordTagsTmp join InfoKeyword on Keyword=Word
                on duplicate key update Weight=values(Weight);
                
                drop table __StoryKeywordTagsTmp;
                commit;";
            Helpers.RunSqlStatement(Name, sql, false);

            //Add geotag to stories where the standard deviation of the first two hours' tweets in the story is low
            sql =
                @"insert into Story (StoryID, Latitude, Longitude)
                select 
	                s.StoryID, avg(t.Latitude) Latitude, avg(t.Longitude) Longitude
                from Story s
                join TweetCluster tc on tc.StoryID=s.StoryID
                join Tweet t on t.TweetClusterID=tc.TweetClusterID
                where s.PendingUpdate and t.RetweetOf is null and s.StartTime > utc_timestamp() - interval 2 hour
                group by s.StoryID
                having count(t.longitude)>2 and stddev(t.longitude) < 3 and stddev(t.latitude) < 3
                on duplicate key update Latitude=values(Latitude), Longitude=values(Longitude)";
            Helpers.RunSqlStatement(Name, sql, false);

            //Reset dirty reads
            Helpers.RunSqlStatement(Name, "SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ", false);
        }

        static void ProcessTweetMetatags()
        {
            string sql =
                @"insert into StoryAidrAttributeTag (StoryID, AttributeID, LabelID, TagCount)
                select s.StoryID, AttributeID, LabelID, count(*) as TagCount
                from TweetAidrAttributeTag tag
                join Tweet t on t.TweetID=tag.TweetID
                join TweetCluster tc on tc.TweetClusterID=t.TweetClusterID
                join Story s on s.StoryID=tc.StoryID
                where s.PendingUpdate and not ProcessedMetatags
                group by s.StoryID
                on duplicate key update TagCount=TagCount+values(TagCount)";
            Helpers.RunSqlStatement(Name, sql, false);

            Helpers.RunSqlStatement(Name, "update Tweet set ProcessedMetatags=1 where not ProcessedMetatags and TweetClusterID is not null", false);

            //Update IsMajorityLabel
            sql =
                @"update StoryAidrAttributeTag tag
	            join Story on Story.StoryID=tag.StoryID 
                set IsMajorityTag=0 
                where Story.PendingUpdate and IsMajorityTag;

                update StoryAidrAttributeTag
                natural join (
	                select tag.StoryID, AttributeID, max(tag.TagCount) as TagCount
	                from StoryAidrAttributeTag tag
	                join Story on Story.StoryID=tag.StoryID
                    where PendingUpdate
	                group by StoryID, AttributeID) T
                set IsMajorityTag=1;";
            Helpers.RunSqlStatement(Name, sql, true);
        }

        static void DoDBMaintenance()
        {
            //Truncate story merge candidates
            Helpers.RunSqlStatement(Name, "truncate TweetClusterCollision", false);

            //Archive stories
            int archivedStoriesCount = Helpers.RunSqlStatement(Name,
                @"set @archivetime = (select max(StartTime) - interval 4 hour from TweetCluster where StartTime < utc_timestamp());
                update Story set IsArchived=1 where not IsArchived and EndTime < @archivetime;");
            Console.WriteLine("Archived " + archivedStoriesCount + " Stories");

            //Delete from WordTweet where story was just archived
            Helpers.RunSqlStatement(Name,
                @"delete wt.* from 
                (select max(StartTime) - interval 1 day archivetime from TweetCluster where StartTime < utc_timestamp()) atime,
                WordTweet wt natural join Tweet natural join TweetCluster join Story on Story.StoryID=TweetCluster.StoryID
                where Story.IsArchived and Story.EndTime < @archivetime;", false);

            //Delete TweetClusters that have no StoryID and no word vector (can't do anything with them)
            Helpers.RunSqlStatement(Name,
                @"delete tc.*
                from TweetCluster tc
                    left join Tweet t on tc.TweetClusterID = t.TweetClusterID
                    left join WordTweet wt on wt.TweetID = t.TweetID
                where StoryID is null
                and wt.TweetID is null;", false);

            //Delete small insignificant stuff
            int deletedStoryCount = Helpers.RunSqlStatement(Name,
                @"delete s.* 
                from Story s, (select max(StartTime) processtime from TweetCluster where StartTime < utc_timestamp()) pt where 
	                IsArchived 
	                and WeightedSize < pow(greatest(0, least(500000, (unix_timestamp(processtime)-unix_timestamp(EndTime))/86400)), 1.3)
	                and not exists (select 1 from StoryInfoCategoryTag t where t.StoryID=s.StoryID limit 1)
	                and not exists (select 1 from StoryInfoEntityTag t where t.StoryID=s.StoryID limit 1)
	                and not exists (select 1 from StoryLocationTag t where t.StoryID=s.StoryID limit 1);", false);
            Console.WriteLine("Deleted " + deletedStoryCount + " old or small stories.");

            //Delete TweetClusters that don't have any words associated with them and don't have a StoryID (can't do anything with them)
            Helpers.RunSqlStatement(Name,
                "delete ik.* from InfoKeyword ik left join StoryInfoKeywordTag tag on tag.InfoKeywordID = ik.InfoKeywordID where tag.InfoKeywordID is null;", false);

            //TweetUrl is a MyISAM table and nothing is deleted automatically
            Helpers.RunSqlStatement(Name,
                "delete tu.* from TweetUrl tu left join Tweet t on t.TweetID = tu.TweetID where t.TweetID is null;", false);
        }

        public static void ApplyPendingStorySplits()
        {
            //Get split actions
            List<SplitAction> splitActions = new List<SplitAction>();
            Helpers.RunSelect(Name, "select StoryID, TweetClusterID, IP, UserID from StorySplits where SplitAt is null;",
                splitActions,
                (values, reader) =>
                {
                    long storyID = reader.GetInt64("StoryID");
                    long clusterID = reader.GetInt64("TweetClusterID");
                    long ip = reader.GetInt64("IP");
                    long userID = reader.GetInt64("UserID");

                    splitActions.Add(new SplitAction() { StoryID = storyID, TweetClusterID = clusterID });

                    string sql = @"insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, TweetCount, RetweetCount, UserCount, TopUserCount) 
                        select " + ip + "," + userID + @", utc_timestamp(), 13, unix_timestamp(utc_timestamp())-unix_timestamp(StartTime), StoryID, TweetCount, RetweetCount, UserCount, TopUserCount
                        from Story where StoryID = " + storyID + ";";
                    Helpers.RunSqlStatement(Name, sql);
                }
            );

            //Perform split actions
            SplitStories(splitActions);

            //Delete processed actions
            foreach (var split in splitActions)
                Helpers.RunSqlStatement(Name, "delete from StorySplits where StoryID=" + split.StoryID + " and TweetClusterID=" + split.TweetClusterID, false);
        }

        public static void ApplyPendingStoryMerges()
        {
            Helpers.RunSqlStatement(Name, "update ignore StoryMerges s1 join StoryMerges s2 on s2.StoryID1=s1.StoryID2 set s2.StoryID1=s1.StoryID1", false);

            //Get user-initiated merge actions. The actions are grouped as a user may have merged stories recursively.
            Dictionary<long, HashSet<long>> mergeGroups = new Dictionary<long, HashSet<long>>();
            Helpers.RunSelect(Name, "select StoryID1, StoryID2, IP, UserID from StoryMerges where MergedAt is null order by StoryID2 desc;",
                mergeGroups,
                (values, reader) =>
                {
                    long keepID = reader.GetInt64("StoryID1");
                    long mergeID = reader.GetInt64("StoryID2");
                    if (mergeGroups.ContainsKey(mergeID))
                    {
                        HashSet<long> tmp = mergeGroups[mergeID];
                        tmp.Add(mergeID);
                        mergeGroups.Remove(mergeID);
                        if (mergeGroups.ContainsKey(keepID))
                            mergeGroups[keepID].UnionWith(tmp);
                        else
                            mergeGroups.Add(keepID, tmp);
                    }
                    else if (mergeGroups.ContainsKey(keepID))
                    {
                        mergeGroups[keepID].Add(mergeID);
                    }
                    else
                    {
                        mergeGroups.Add(keepID, new HashSet<long>());
                        mergeGroups[keepID].Add(mergeID);
                    }
                    long ip = reader.GetInt64("IP");
                    long userID = reader.GetInt64("UserID");

                    string sql = @"insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, MergedWithStoryID, TweetCount, RetweetCount, UserCount, TopUserCount) 
                        select " + ip + "," + userID + ", utc_timestamp(), 12, unix_timestamp(utc_timestamp())-unix_timestamp(StartTime), StoryID, " + keepID + @", TweetCount, RetweetCount, UserCount, TopUserCount
                        from Story where StoryID = " + mergeID + ";";
                    Helpers.RunSqlStatement(Name, sql, false);
                }
            );

            var mergeActions = mergeGroups.SelectMany(n =>
                n.Value.Select(m => new MergeAction() { KeptStoryID = n.Key, MergedStoryID = m }));
            
            //Perform merge actions
            MergeStories(mergeActions, isAutomerge: false);

            //Clean old actions
            Helpers.RunSqlStatement(Name, "delete from StoryMerges where MergedAt is not null and MergedAt < (utc_timestamp() - interval 1 hour);", false);
        }

        public static void MarkStoriesAsProcessed()
        {
            Helpers.RunSqlStatement(Name, "update Story set PendingUpdate=0 where PendingUpdate=1", false);
            Helpers.RunSqlStatement(Name, "update TweetCluster set PendingStoryUpdate=0 where PendingStoryUpdate=1", false);
        }
    }
}
