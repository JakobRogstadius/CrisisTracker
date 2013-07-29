/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using CrisisTracker.Common;

namespace CrisisTracker.TweetParser
{
    public class PeriodicMaintenance
    {
        public static string Name { get; set; }

        const int TEN_MINUTES_MS = 1000 * 60 * 10;

        public static void Run()
        {
            Name = "PeriodicMaintenance";
            //Console.WriteLine(Name);
            Console.Write("Performing maintenance");

            try
            {
                //Decrease filter performances
                //0.995198 every 10 minutes gives 50% decrease per day
                Helpers.RunSqlStatement(Name, "update TwitterTrackFilter set Hits1d = Hits1d * 0.995198, Discards1d = Discards1d * 0.995198 where IsActive;", false);
                Console.Write(".");

                //Decrease word scores
                //0.890899 every 10 minutes gives 50% decrease per hour
                //0.99879734 every 10 minutes gives 50% decrease per four days
                Helpers.RunSqlStatement(Name, "update WordScore set Score1h = Score1h * 0.890899, Score4d = Score4d * 0.99879734;", false);
                Console.Write(".");

                //Remove all references to words with too low scores
                //Delete if word has been mentioned less than 100 times in four days and no time in the past two hours
                Helpers.RunSqlStatement(Name, "create temporary table tmpWordsToDelete (WordID bigint(20) unsigned not null, primary key (WordID)) select WordID from WordScore where (score4d < 50 and score1h < 0.5) or score1h < 0.2;", false);
                Helpers.RunSqlStatement(Name, "delete w.* from Word w, tmpWordsToDelete wd where wd.WordID = w.WordID;", false); //Trigger on Word deletes from WordScore and WordTweet
                Helpers.RunSqlStatement(Name, "drop table tmpWordsToDelete;", false);
                Console.Write(".");

                //Decrease user scores and delete when score is too low
                //0.995198 every 10 minutes gives 50% decrease per 12 hours
                Helpers.RunSqlStatement(Name, "update TwitterUser set Score12h = Score12h * 0.990419 where Score12h >= 0.5;", false);
                Console.Write(".");
                Helpers.RunSqlStatement(Name, "delete from TwitterUser where not exists (select 1 from Tweet where Tweet.UserID = TwitterUser.UserID limit 1) and Score12h < 0.5;", false);
                Console.Write(".");
                //Update top user index
                Helpers.RunSqlStatement(Name,
                    @"update TwitterUser u,
                    (select min(Score12h) Ts from (select Score12h from TwitterUser order by 1 desc limit 5000) T) T
                    set IsTopUser = (Score12h >= Ts)
                    where Score12h >= Ts or IsTopUser;
                    ", false);
                Console.Write(".");

                //Update stop word score threshold
                double ratio = Settings.TweetParser_WordScore4dMaxToStopwordRatio;
                Helpers.RunSqlStatement(Name, "update Constants set value = (select " + ratio + " * max(score4d) from WordScore) where name = 'WordScore4dHigh';", false);
                Console.Write(".");

                //Clean up tweets and references to tweets that have become too old

                //Prune the WordTweet table of references to stop words
                string deleteStopWordReferencesSql = @"delete wt.* 
                        from WordTweet wt, 
                            WordScore ws, 
                            Word w
                        where
                            wt.WordID = ws.WordID 
                            and wt.WordID = w.WordID
                            and coalesce(
                                if(Word like '#%', 1, null), 
                                (select 1 from TwitterTrackFilter where IsActive and FilterType = 0 and Word = w.Word limit 1), 
                                0) = 0
                            and ws.Score4d > (select value from Constants where name = 'WordScore4dHigh');";
                Helpers.RunSqlStatement(Name, deleteStopWordReferencesSql, false);
                Console.Write(".");

                //Delete words that lack a word score
                Helpers.RunSqlStatement(Name,
                    "delete Word.* from Word left join WordScore on WordScore.WordID = Word.WordID where WordScore.WordID is null;", false);

                ////Prune the WordTweet table if it has grown too large
                //int maxLength = Convert.ToInt32(ConfigurationManager.AppSettings["maxWordTweetTableLength"].ToString());
                //List<int> rowCount = new List<int>();
                //Helpers.RunSelect(Name, _connectionString, "select count(*) from WordTweet;", rowCount, (values, reader) => values.Add(Convert.ToInt32(reader[0])));
                //if (rowCount.Count == 1 && rowCount[0] > maxLength)
                //{
                //    /* 120 rows
                //     * 100 threshold
                //     * delete when rand() > 100/120
                //     */
                //    double deleteRatio = (double)maxLength / Convert.ToInt32(rowCount[0]);
                //    Helpers.RunSqlStatement(Name, _connectionString, "delete from WordTweet where rand() > " + deleteRatio + ";");
                //    Console.Write(".");
                //}

                Console.WriteLine();
            }
            catch (Exception e)
            {
                Output.Print(Name, "Exception while performing maintenance" + Environment.NewLine + e);
            }
        }
    }
}
