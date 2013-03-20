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
    public class StoryWorker
    {
        class SimpleTweetCluster
        {
            public long TweetClusterID { get; set; }
            public long? StoryID { get; set; }
            public WordVector Vector { get; set; }

            public SimpleTweetCluster()
            {
                Vector = new WordVector();
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
            public WordVector Vector { get; set; }
            public bool Changed { get; set; }
            public bool Created { get; set; }
            public long? MergedWith { get; set; }

            public SimpleStory()
            {
                Vector = new WordVector();
            }

            public override bool Equals(object obj)
            {
                if (obj is SimpleStory)
                    return StoryID.Equals(((SimpleStory)obj).StoryID);
                return false;
            }

            public override int GetHashCode()
            {
                return StoryID.GetHashCode();
            }
        }

        const string Name = "StoryWorker";

        public static void Run()
        {
            try
            {
                //Get next story id
                long nextStoryID = GetNextStoryID();

                Dictionary<long, SimpleTweetCluster> tweetClusters;
                do {
                    tweetClusters = GetUnassignedTweetClusters();
                    Console.WriteLine("Fetched " + tweetClusters.Count + " tweet clusters");

                    if (tweetClusters.Count > 0)
                    {
                        Dictionary<long, SimpleStory> stories = GetStories();
                        Console.WriteLine("Fetched " + stories.Count + " stories");

                        AssignTweetClustersToNearestStory(tweetClusters, stories, ref nextStoryID);
                        Console.WriteLine("Assigned stories to clusters.");

                        SaveToDB(tweetClusters, stories);
                        Console.WriteLine("Stored results in DB.");

                        //AutoMergeSimilarStories(stories.Where(n => n.Value.Created).Select(n => n.Key).ToList());
                    }
                } while (tweetClusters.Count > Settings.TweetClusterer_SW_TweetClusterBatchSize * 0.9);

                Console.WriteLine("StoryWorker complete!");
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
                        from (select TweetClusterID from TweetCluster where StoryID is null order by 1 limit " + Settings.TweetClusterer_SW_TweetClusterBatchSize + @") as TweetCluster
                            natural join Tweet natural join WordTweet natural join WordScore
                        group by TweetClusterID, WordID
                        order by TweetClusterID, Weight desc
                    ) TT
                ) TTT
                where RowNum <= " + Settings.TweetClusterer_SW_MaxWordsInStoryVector + @"
                ;";

            //Parse returned word weights and build TweetCluster vectors
            Dictionary<long, SimpleTweetCluster> clusters = new Dictionary<long, SimpleTweetCluster>();
            Helpers.RunSelect(Name, query, clusters, (values, reader) =>
            {
                long clusterID = reader.GetInt64("TweetClusterID");
                if (!clusters.ContainsKey(clusterID))
                    clusters.Add(clusterID, new SimpleTweetCluster() { TweetClusterID = clusterID });

                values[reader.GetInt64("TweetClusterID")].Vector.AddItem(
                    reader.GetInt64("WordID"),
                    reader.GetDouble("Weight"));
            });

            //Normalize vectors
            foreach (SimpleTweetCluster cluster in clusters.Values)
                cluster.Vector.Normalize();

            return clusters;
        }

        static Dictionary<long, SimpleStory> GetStories()
        {
            //Get word vector for each story
            string query = @"
                select StoryID, WordID, Weight from (
                select
                    TT.*,
                    @r := IF(@g=StoryID,@r+1,1) RowNum,
                    @g := StoryID
                from 
                    (select @g:=null) initvars
                    cross join
                    (
                        select
                            T.StoryID,
                            WordID, 
                            count(*) * ScoreToIdf(Score4d) as Weight
                        from (select StoryID from Story where UserCount > 2 and not IsArchived order by EndTime desc limit " + Settings.TweetClusterer_SW_CandidateStoryCount + @") T
                            join TweetCluster tc on tc.StoryID = T.StoryID
                            natural join Tweet natural join WordTweet natural join WordScore
                        group by TweetClusterID, WordID
                        order by TweetClusterID, Weight desc
                    ) TT
                ) TTT
                where RowNum <= " + Settings.TweetClusterer_SW_MaxWordsInStoryVector + @"
                ;";

            //Parse returned word weights and build story vectors
            Dictionary<long, SimpleStory> stories = new Dictionary<long, SimpleStory>();
            Helpers.RunSelect(Name, query, stories, (values, reader) =>
            {
                long storyID = reader.GetInt64("StoryID");
                if (!stories.ContainsKey(storyID))
                    stories.Add(storyID, new SimpleStory() { StoryID = storyID });

                values[storyID].Vector.AddItem(
                    reader.GetInt64("WordID"),
                    reader.GetDouble("Weight"));
            });

            //Normalize vectors
            foreach (SimpleStory story in stories.Values)
                story.Vector.Normalize();

            return stories;
        }

        public static void ApplyPendingStorySplits()
        {
            //Get split actions
            Dictionary<long, HashSet<long>> splitActions = new Dictionary<long, HashSet<long>>();
            Helpers.RunSelect(Name, "select StoryID, TweetClusterID, IP, UserID from PendingStorySplits where SplitAt is null;",
                splitActions,
                (values, reader) =>
                {
                    long storyID = reader.GetInt64("StoryID");
                    long clusterID = reader.GetInt64("TweetClusterID");
                    if (splitActions.ContainsKey(storyID))
                    {
                        splitActions[storyID].Add(clusterID);
                    }
                    else
                    {
                        splitActions.Add(storyID, new HashSet<long>());
                        splitActions[storyID].Add(clusterID);
                    }
                    long ip = reader.GetInt64("IP");
                    long userID = reader.GetInt64("UserID");

                    string sql = @"insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend) 
                        select " + ip + "," + userID + @", utc_timestamp(), 13, unix_timestamp(utc_timestamp())-unix_timestamp(StartTime), StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend
                        from Story where StoryID = " + storyID + ";";
                    Helpers.RunSqlStatement(Name, sql);
                }
            );

            //Perform split actions
            SplitStories(splitActions);

            //Mark actions as handled
            foreach (var item in splitActions)
            {
                foreach (var clusterID in item.Value)
                {
                    Helpers.RunSqlStatement(Name, "update PendingStorySplits set SplitAt = utc_timestamp() where StoryID=" + item.Key + " and TweetClusterID=" + clusterID + ";", false);
                }
            }

            //Clean old actions
            Helpers.RunSqlStatement(Name, "delete from PendingStoryMerges where MergedAt is not null and MergedAt < (utc_timestamp() - interval 30 minute);", false);
        }

        public static void ApplyPendingStoryMerges()
        {
            //Get merge actions
            Dictionary<long, HashSet<long>> mergeActions = new Dictionary<long, HashSet<long>>();
            Helpers.RunSelect(Name, "select StoryID1 as id1, StoryID2 as id2, IP, UserID from PendingStoryMerges where MergedAt is null;",
                mergeActions,
                (values, reader) => {
                    long id1 = reader.GetInt64("id1");
                    long id2 = reader.GetInt64("id2");
                    if (mergeActions.ContainsKey(id2))
                    {
                        HashSet<long> tmp = mergeActions[id2];
                        tmp.Add(id2);
                        mergeActions.Remove(id2);
                        if (mergeActions.ContainsKey(id1))
                            mergeActions[id1].UnionWith(tmp);
                        else
                            mergeActions.Add(id1, tmp);
                    }
                    else if (mergeActions.ContainsKey(id1))
                    {
                        mergeActions[id1].Add(id2);
                    }
                    else
                    {
                        mergeActions.Add(id1, new HashSet<long>());
                        mergeActions[id1].Add(id2);
                    }
                    long ip = reader.GetInt64("IP");
                    long userID = reader.GetInt64("UserID");

                    string sql = @"insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, MergedWithStoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend) 
                        select " + ip + "," + userID + ", utc_timestamp(), 12, unix_timestamp(utc_timestamp())-unix_timestamp(StartTime), StoryID, " + id1 + @", TweetCount, RetweetCount, UserCount, TopUserCount, Trend
                        from Story where StoryID = " + id2 + ";";
                    Helpers.RunSqlStatement(Name, sql, false);
                }
            );

            //Perform merge actions
            MergeStories(mergeActions);

            //Mark actions as handled
            foreach (var item in mergeActions)
            {
                foreach (var id2 in item.Value)
                {
                    Helpers.RunSqlStatement(Name, "update PendingStoryMerges set MergedAt = utc_timestamp() where StoryID1=" + item.Key + " and StoryID2=" + id2 + ";", false);
                }
            }

            //Clean old actions
            Helpers.RunSqlStatement(Name, "delete from PendingStoryMerges where MergedAt is not null and MergedAt < (utc_timestamp() - interval 30 minute);", false);
        }

        static void AssignTweetClustersToNearestStory(Dictionary<long, SimpleTweetCluster> clusters, Dictionary<long, SimpleStory> stories, ref long nextStoryID)
        {
            //Assign batch clusters to nearest story
            foreach (SimpleTweetCluster cluster in clusters.Values.OrderBy(n => n.TweetClusterID))
            {
                List<long> nearStories = new List<long>();
                if (stories.Count > 0)
                {
                    var distances = stories.Values
                        .Select(n => new { StoryID = n.StoryID, Distance = n.Vector * cluster.Vector })
                        .OrderByDescending(n => n.Distance);
                    double prevDist = double.MaxValue;
                    List<long> candidates = new List<long>();
                    foreach (var item in distances)
                    {
                        if (prevDist == double.MaxValue)
                            prevDist = item.Distance;

                        if (item.Distance > Settings.TweetClusterer_SW_MergeUpperThreshold) //Similar enough
                        {
                            nearStories.Add(item.StoryID);
                        }
                        else if (item.Distance > Settings.TweetClusterer_SW_MergeLowerThreshold
                            && item.Distance > prevDist * Settings.TweetClusterer_SW_MergeDropScale) //Still looking for a drop
                        {
                            candidates.Add(item.StoryID);
                        }
                        else if (item.Distance <= prevDist * Settings.TweetClusterer_SW_MergeDropScale) //Found a drop
                        {
                            nearStories.AddRange(candidates);
                            candidates.Clear(); //Don't break, as it is theoretically possible to find multiple drops

                            if (item.Distance > Settings.TweetClusterer_SW_MergeLowerThreshold)
                                candidates.Add(item.StoryID);
                            else
                                break;
                        }
                        else //if (item.Distance < _storyMergeLowerThreshold) //Didn't find a drop
                        {
                            break;
                        }

                        prevDist = item.Distance;
                    }
                }

                //foreach (SimpleStory story in stories.Values)
                //{
                //    double sim = story.Vector * cluster.Vector;
                //    double ratio = sim > 0.1 ? story.Vector.SharedWordRatio(cluster.Vector) : 0;
                //    if (sim > GetSimilarityThreshold(story.Vector.ItemCount) || ratio >= 0.6)
                //        nearStories.Add(story.StoryID);
                //}

                long storyID = -1;
                if (nearStories.Count == 0) //Create new story
                {
                    storyID = nextStoryID++;
                    stories.Add(storyID, new SimpleStory() { StoryID = storyID, Vector = cluster.Vector, Changed = true, Created = true });
                }
                else if (nearStories.Count == 1) //Add to story
                {
                    storyID = nearStories[0];
                }
                else //Merge multiple stories
                {
                    storyID = nearStories.Min();
                    stories[storyID].Vector = WordVector.GetNormalizedAverage(nearStories.Select(n => stories[n].Vector), Settings.TweetClusterer_SW_MaxWordsInStoryVector);
                    
                    foreach (long deleteStoryID in nearStories.Where(n => n != storyID))
                    {
                        stories[deleteStoryID].MergedWith = storyID;
                        foreach (SimpleTweetCluster clusterToMove in clusters.Values.Where(n => n.StoryID == deleteStoryID))
                            clusterToMove.StoryID = storyID;
                    }
                }

                cluster.StoryID = storyID;
                stories[storyID].Changed = true;
            }
        }

        static float GetSimilarityThreshold(int termCount)
        {
            ////Similarity threshold: http://www.wolframalpha.com/input/?i=plot+0.5+%2B+0.3%2F%281%2Bx*0.01%29+from+5+to+200
            //return (float)(0.45 + 0.25 / (1 + termCount * 0.04));
            return 0.55f;
        }

        static void SaveToDB(Dictionary<long, SimpleTweetCluster> clusters, Dictionary<long, SimpleStory> stories)
        {
            //Insert new Stories (otherwise the TweetCluster update below will fail from trigger constraints)
            string tweetClusterInsert = "INSERT IGNORE INTO Story (StoryID, StartTime, EndTime) VALUES (" +
                string.Join(",'3000-01-01',0),(", clusters.Values.Select(n => n.StoryID).Distinct().Select(n => n.ToString()).ToArray()) + ",'3000-01-01',0);";
            Helpers.RunSqlStatement(Name, tweetClusterInsert, false);


            //Assign StoryID to clusters
            StringBuilder sbClusters = new StringBuilder();
            sbClusters.AppendLine("insert into TweetCluster (TweetClusterID, StoryID) values");
            bool firstCluster = true;
            foreach (SimpleTweetCluster cluster in clusters.Values)
            {
                if (firstCluster) firstCluster = false;
                else sbClusters.AppendLine(",");

                sbClusters.Append("(" + cluster.TweetClusterID + "," + cluster.StoryID + ")");
            }
            sbClusters.AppendLine();
            sbClusters.AppendLine("on duplicate key update StoryID=VALUES(StoryID);");

            int affected = Helpers.RunSqlStatement(Name, sbClusters.ToString(), false);
            Console.WriteLine("Updated StoryID of " + clusters.Count + " clusters (" + affected + " rows affected)");


            //Update tweet clusters to reflect story merges
            if (stories.Values.Any(n => n.MergedWith.HasValue))
            {
                Dictionary<long, HashSet<long>> mergeActions = new Dictionary<long, HashSet<long>>();
                foreach (var story in stories.Values.Where(n => n.MergedWith.HasValue))
                {
                    if (!mergeActions.ContainsKey(story.MergedWith.Value))
                        mergeActions.Add(story.MergedWith.Value, new HashSet<long>());
                    mergeActions[story.MergedWith.Value].Add(story.StoryID);
                }

                //Log merge actions
                Helpers.RunSqlStatement(Name, 
                    @"INSERT INTO HourStatistics (DateHour, StoriesAutoMerged) 
                    VALUES (DATE_FORMAT(utc_timestamp(), ""%Y-%m-%d %H:00:00""), " + mergeActions.Sum(n => n.Value.Count) + @")
                    ON DUPLICATE KEY UPDATE StoriesAutoMerged = StoriesAutoMerged + values(StoriesAutoMerged);", false);

                MergeStories(mergeActions);
            }


            //Update all Story counts that may have changed, from recent additions or by moving out of the 4h time window
            string sqlStoryCounts = @"
                insert into Story (StoryID, TweetCount, TweetCountRecent, RetweetCount, RetweetCountRecent, UserCount, UserCountRecent, TopUserCount, TopUserCountRecent, StartTime, EndTime)
                select
                    s.StoryID,
                    sum(tc.TweetCount),
                    sum(tc.TweetCountRecent),
                    sum(tc.RetweetCount),
                    sum(tc.RetweetCountRecent),
                    sum(tc.UserCount),
                    sum(tc.UserCountRecent),
                    sum(tc.TopUserCount),
                    sum(tc.TopUserCountRecent),
                    min(tc.StartTime),
                    max(tc.EndTime)
                from
                    Story s
                    join TweetCluster tc on tc.StoryID=s.StoryID
                where s.UserCountRecent > 0 or s.StartTime > utc_timestamp()
                group by s.StoryID
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
            Helpers.RunSqlStatement(Name, sqlStoryCounts, false);

            //Update Story titles
            Helpers.RunSqlStatement(Name, @"
                update Story s
                set s.Title = (select Title from TweetCluster tc where tc.StoryID=s.StoryID order by UserCount desc limit 1)
                where s.Title = '' or s.EndTime > (select max(EndTime) - interval 10 minute from TweetCluster)
                ;", false);

            //Update Story trends
            int trendUpdateCount = Helpers.RunSqlStatement(Name, @"
                insert into Story (StoryID, Trend, MaxGrowth)
                select StoryID, N0-N1 as Trend, N0 as MaxGrowth from (
                    select 
                        s.*, 
                        count(distinct case when CreatedAt between T2 and T0 then UserID else null end) as N0, 
                        count(distinct case when CreatedAt between T3 and T1 then UserID else null end) as N1
                    from Story s
                        join TweetCluster tc on tc.StoryID = s.StoryID
                        natural join Tweet t,
                        (select 
                            max(CreatedAt) as T0, 
                            max(CreatedAt) - interval 15 minute T1, 
                            max(CreatedAt) - interval 30 minute T2, 
                            max(CreatedAt) - interval 45 minute T3 from Tweet) T
                    where s.EndTime > T3 or s.Trend != 0
                    group by s.StoryID
                    having N0 > 0 or s.Trend > 0
                ) TT
                on duplicate key update Trend=VALUES(Trend), MaxGrowth=if(VALUES(MaxGrowth)>Story.MaxGrowth,VALUES(MaxGrowth),Story.MaxGrowth);", false);
            Console.WriteLine("Updated " + trendUpdateCount + " trends");

            //Add tags for newly created stories
            string newStoryIDsStr = string.Join(",", stories.Where(n => n.Value.Created).Select(n => n.Key.ToString()).ToArray());
            if (newStoryIDsStr != "")
            {
                affected = Helpers.RunSqlStatement(Name, @"
                    start transaction;
                    create table __StoryKeywordTagsTmp as
                    select StoryID, Word
                    from (
                        select T.*,
                            @r := IF(@g=StoryID,@r+1,1) RowNum,
                            @s := IF(@g=StoryID,@s+Score,Score) AccuScore,
                            @c := IF(@g=StoryID,@c+Occurences,Occurences) AccuOcc,
                            @g := StoryID
                        from
                            (select @g:=null, @r:=0, @s:=0, @c:=0) initvars
                            CROSS JOIN (
                            select
                                StoryID, 
                                Word,
                                count(*) as Occurences,
                                count(*) * ScoreToIdf(Score4d) as Score,
                                if(Word like '#%' or (select 1 from TwitterTrackFilter where FilterType = 0 and Word = Word.Word and IsActive limit 1), 1, 0) as TopicWord
                            from
                                TweetCluster
                                natural join Tweet
                                natural join WordTweet
                                natural join Word
                                natural join WordScore
                            where StoryID in (" + newStoryIDsStr + @")
                            group by StoryID, WordID
                        ) T
                        order by StoryID,Score desc
                    ) T
                    where Score/AccuScore>0.05 or (TopicWord and Occurences/AccuOcc > 0.05);
                    insert ignore into InfoKeyword (Keyword) select Word from __StoryKeywordTagsTmp;
                    insert ignore into StoryInfoKeywordTag (StoryID, InfoKeywordID, CreatedAt, UserID) 
                    select StoryID, InfoKeywordID, utc_timestamp(), 0 from __StoryKeywordTagsTmp join InfoKeyword on Keyword=Word;
                    drop table __StoryKeywordTagsTmp;
                    commit;");

                Console.WriteLine("Inserted " + affected + " story tags");
            }

            //Archive stories
            int archivedClustersCount = Helpers.RunSqlStatement(Name,
                @"set @archivetime = (select max(StartTime) - interval 4 hour from TweetCluster where StartTime < utc_timestamp());
                update TweetCluster set IsArchived=1 where not IsArchived and StoryID is not null and EndTime < @archivetime;");
            int archivedStoriesCount = Helpers.RunSqlStatement(Name,
                @"set @archivetime = (select max(StartTime) - interval 4 hour from Story where StartTime < utc_timestamp());
                update Story set IsArchived=1 where not IsArchived and EndTime < @archivetime;");
            Console.WriteLine("Archived " + archivedClustersCount + " TweetClusters and " + archivedStoriesCount + " Stories");

            //Delete from WordTweet where story was just archived
            Helpers.RunSqlStatement(Name,
                @"set @archivetime = (select max(StartTime) - interval 1 day from Story where StartTime < utc_timestamp());
                delete wt.* from WordTweet wt natural join Tweet natural join TweetCluster join Story on Story.StoryID=TweetCluster.StoryID
                where Story.IsArchived and Story.EndTime < @archivetime;"); //EndTime < (select max(CreatedAt) from Tweet where CalculatedRelations) - interval 48 hour;");

            //Delete TweetClusters that don't have any words associated with them and don't have a StoryID (can't do anything with them)
            Helpers.RunSqlStatement(Name,
                @"delete tc.*
                from TweetCluster tc
                    left join Tweet t on tc.TweetClusterID = t.TweetClusterID
                    left join WordTweet wt on wt.TweetID = t.TweetID
                where StoryID is null
                and wt.TweetID is null;", false);

            //Delete small insignificant stuff
            int deletedStoryCount = Helpers.RunSqlStatement(Name,
                @"set @processtime = (select max(StartTime) from Story where StartTime < utc_timestamp());
                delete s.* from Story s where 
                    IsArchived 
                    and UserCount < pow((unix_timestamp(@processtime)-unix_timestamp(EndTime))/86400, 1.3)
                    and not exists (select 1 from StoryInfoCategoryTag t where t.StoryID=s.StoryID limit 1)
                    and not exists (select 1 from StoryInfoEntityTag t where t.StoryID=s.StoryID limit 1)
                    and not exists (select 1 from StoryLocationTag t where t.StoryID=s.StoryID limit 1);");
            Console.WriteLine("Deleted " + deletedStoryCount + " old or small stories.");
            Helpers.RunSqlStatement(Name,
                "delete ik.* from InfoKeyword ik left join StoryInfoKeywordTag tag on tag.InfoKeywordID = ik.InfoKeywordID where tag.InfoKeywordID is null;", false);
            //TweetUrl is a MyISAM table and nothing is deleted automatically
            Helpers.RunSqlStatement(Name,
                "delete tu.* from TweetUrl tu left join Tweet t on t.TweetID = tu.TweetID where t.TweetID is null;", false);
        }

        static void SplitStories(Dictionary<long, HashSet<long>> splitActions)
        {
            //Apply split actions
            foreach (var action in splitActions)
            {
                //Set story id to null on removed cluster
                //Delete cluster if archived (no WordTweet data left to work with)
                //Update story

                string clusterIDsStr = string.Join(",", action.Value.Select(n => n.ToString()).ToArray());

                Console.WriteLine("Applying split action: " + action.Key + "->" + clusterIDsStr);

                Helpers.RunSqlStatement(Name,
                    "update TweetCluster set StoryID=null where TweetClusterID in (" + clusterIDsStr + "); " +
                    "delete from TweetCluster where IsArchived and TweetClusterID in (" + clusterIDsStr + "); " +
                    @"update Story st
                            join (
                                select
                                    s.StoryID,
                                    (select Title from TweetCluster where StoryID=s.StoryID order by UserCount desc, StartTime limit 1) as Title,
                                    sum(tc.TweetCount) as TweetCount,
                                    sum(tc.TweetCountRecent) as TweetCountRecent,
                                    sum(tc.RetweetCount) as RetweetCount,
                                    sum(tc.RetweetCountRecent) as RetweetCountRecent,
                                    sum(tc.UserCount) as UserCount,
                                    sum(tc.UserCountRecent) as UserCountRecent,
                                    sum(tc.TopUserCount) as TopUserCount,
                                    sum(tc.TopUserCountRecent) as TopUserCountRecent,
                                    min(tc.StartTime) as StartTime,
                                    max(tc.EndTime) as EndTime,
                                    min(s.IsArchived) as IsArchived
                                from
                                    Story s
                                    join TweetCluster tc on tc.StoryID=s.StoryID,
                                    (select max(EndTime) as T0 from TweetCluster) as TTT
                                where s.StoryID = " + action.Key + @"
                                group by s.StoryID
                            ) T on T.StoryID = st.StoryID
                        set
                            st.Title=T.Title, 
                            st.TweetCount=T.TweetCount,
                            st.TweetCountRecent=T.TweetCountRecent, 
                            st.RetweetCount=T.RetweetCount,
                            st.RetweetCountRecent=T.RetweetCountRecent, 
                            st.UserCount=T.UserCount, 
                            st.UserCountRecent=T.UserCountRecent, 
                            st.TopUserCount=T.TopUserCount, 
                            st.TopUserCountRecent=T.TopUserCountRecent,
                            st.StartTime=T.StartTime, 
                            st.EndTime=T.EndTime,
                            st.IsArchived=T.IsArchived
                        ;");
            }

            Console.WriteLine("Split " + splitActions.Count + " stories.");
        }

        static void MergeStories(Dictionary<long, HashSet<long>> mergeActions)
        {
            //Apply merge actions
            foreach (var action in mergeActions.OrderByDescending(n => n.Key))
            {
                string oldStoryIDsStr = string.Join(",", action.Value.Select(n => n.ToString()).ToArray());

                Console.WriteLine("Applying merge action: " + action.Key + "<-" + oldStoryIDsStr);

                Helpers.RunSqlStatement(Name,
                    "update TweetCluster set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    "update ignore StoryInfoKeywordTag set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    "update ignore StoryInfoCategoryTag set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    "update ignore StoryInfoEntityTag set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    "update ignore StoryLocationTag set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    "update ignore StoryCustomTitle set StoryID=" + action.Key + " where StoryID in (" + oldStoryIDsStr + "); " +
                    @"update Story st
                            join (
                                select
                                    s.StoryID,
                                    (select Title from TweetCluster where StoryID=s.StoryID order by UserCount desc, StartTime limit 1) as Title,
                                    sum(tc.TweetCount) as TweetCount,
                                    sum(tc.TweetCountRecent) as TweetCountRecent,
                                    sum(tc.RetweetCount) as RetweetCount,
                                    sum(tc.RetweetCountRecent) as RetweetCountRecent,
                                    sum(tc.UserCount) as UserCount,
                                    sum(tc.UserCountRecent) as UserCountRecent,
                                    sum(tc.TopUserCount) as TopUserCount,
                                    sum(tc.TopUserCountRecent) as TopUserCountRecent,
                                    min(tc.StartTime) as StartTime,
                                    max(tc.EndTime) as EndTime,
                                    min(s.IsArchived) as IsArchived
                                from
                                    Story s
                                    join TweetCluster tc on tc.StoryID=s.StoryID,
                                    (select max(EndTime) as T0 from TweetCluster) as TTT
                                where s.StoryID = " + action.Key + @"
                                group by s.StoryID
                            ) T on T.StoryID = st.StoryID
                        set
                            st.Title=T.Title, 
                            st.TweetCount=T.TweetCount,
                            st.TweetCountRecent=T.TweetCountRecent, 
                            st.RetweetCount=T.RetweetCount,
                            st.RetweetCountRecent=T.RetweetCountRecent, 
                            st.UserCount=T.UserCount, 
                            st.UserCountRecent=T.UserCountRecent, 
                            st.TopUserCount=T.TopUserCount, 
                            st.TopUserCountRecent=T.TopUserCountRecent,
                            st.StartTime=T.StartTime, 
                            st.EndTime=T.EndTime,
                            st.IsArchived=T.IsArchived
                        ;
                    insert into Story (StoryID, MaxGrowth)
                    select StoryID, max(UserCount) as MaxGrowth from (
                        select
                            s.StoryID,
                            date(t.CreatedAt) as d,
                            hour(t.CreatedAt) as h,
                            count(distinct t.UserID) as UserCount
                        from Story s
                            join TweetCluster tc on tc.StoryID = s.StoryID
                            natural join Tweet t
                            left join TwitterUser u on u.UserID=t.UserID
                        where s.StoryID=" + action.Key + @" and not coalesce(IsBlacklisted,0)
                        group by d,h
                    ) T
                    on duplicate key update MaxGrowth=if(VALUES(MaxGrowth)>Story.MaxGrowth,VALUES(MaxGrowth),Story.MaxGrowth);
                    ");
            }

            if (mergeActions.Count > 0)
            {
                string storyIDStr = string.Join(",", mergeActions.Values.Select(n => 
                    string.Join(",", n.Select(m => m.ToString()).ToArray())).ToArray());
                Helpers.RunSqlStatement(Name, "delete from Story where StoryID in (" + storyIDStr + ");", false); //Trigger on story deletes from tag tables
            }

            Console.WriteLine("Merged " + mergeActions.Count + " story pairs.");
        }
    }
}
