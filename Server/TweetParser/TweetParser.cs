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
using System.Threading;
using MySql.Data.MySqlClient;
using Procurios.Public;
using System.Collections;
using CrisisTracker.Common;
using System.Globalization;

namespace CrisisTracker.TweetParser
{
    class TweetParser
    {
        class Word
        {
            public long ID { get; set; }
            public bool IsStopWord { get; set; }
            public double Idf { get; set; }
        }

        class UserMention
        {
            public long UserID { get; set; }
            public string ScreenName { get; set; }
            public string RealName { get; set; }
            public int Count { get; set; }
        }

        class FilterPerformance
        {
            public int Matches { get; set; }
            public int Discards { get; set; }
        }

        const int TEN_MINUTES_MS = 1000 * 60 * 10;

        DateTime _lastMaintenanceTime = DateTime.MinValue;
        DateTime _lastTweetTime = DateTime.MinValue;

        public string Name { get; set; }

        public TweetParser()
        {
            Name = "TweetParser";
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    DateTime start = DateTime.Now;

                    //Get current tracking filters from database
                    Console.Write("|");
                    List<TrackFilter> filters = GetActiveTrackFilters();
                    Console.Write(".");

                    //Get a batch of tweets
                    Dictionary<long, string> randomTweetsJson = new Dictionary<long, string>();
                    Dictionary<long, string> filteredTweetsJson = new Dictionary<long, string>();
                    GetTweets(filteredTweetsJson, randomTweetsJson);
                    Console.Write(".");

                    //Parse the Json
                    List<Hashtable> filteredTweets = ParseJsonTweets(filteredTweetsJson, onlyExtractWords: false); //Tweets are appended with "created_at_datetime"
                    List<Hashtable> randomTweets = ParseJsonTweets(randomTweetsJson, onlyExtractWords: true); //Tweets only contain a "text" field
                    Console.Write(".");

                    //Extract (stemmed) words from tweets
                    WordCount wordCounts = new WordCount();
                    ExtractWords(randomTweets, wordCounts);
                    ExtractWords(filteredTweets, wordCounts); //Tweets are appended with a "words" string array
                    Console.Write(".");

                    //Insert words into Word and WordScores
                    Dictionary<string, Word> wordInfo = null;
                    if (wordCounts.HasWords)
                        wordInfo = InsertWords(wordCounts);
                    Console.Write(".");

                    if (filteredTweets.Count > 0)
                    {
                        //Count number of tweets per hour
                        StoreTweetCountPerHour(filteredTweets);
                        Console.Write(".");

                        //Make note of the time of the last tweet
                        _lastTweetTime = (DateTime)filteredTweets.Last()["created_at_datetime"];

                        //Remove the off-topic tweets from further processing
                        var filterPerf = RemoveOffTopicTweets(filteredTweets, filters); //Tweets are appended with longitude and latitude
                        Console.Write(".");

                        //Insert filter performance
                        InsertFilterPerformance(filterPerf);
                        Console.Write(".");

                        //Extract URLs
                        ExtractUrls(filteredTweets); //Tweets are appended with a "urls" string array
                        Console.Write(".");

                        //Insert into Tweet, TwitterUser, WordTweet, TweetUrl
                        MySqlCommand bigInsertCommand = BuildInsertSql(filteredTweets, wordInfo);
                        Console.Write(".");

                        int rowCount = Helpers.RunSqlStatement(Name, bigInsertCommand);
                        Console.Write(".");
                    }

                    if (filteredTweets.Count + randomTweetsJson.Count > 0)
                    {
                        int rowsDeleted = DeleteParsedJsonTweets(randomTweetsJson.Keys.Union(filteredTweetsJson.Keys));
                    }
                    Console.WriteLine(".");
                    
                    //Possibly perform maintenance
                    PerformMaintenance();

                    //Wait for up to 30 seconds, unless there are more tweets in the database to process
                    int runtime = (int)(DateTime.Now - start).TotalMilliseconds;
                    if (runtime < 30000 && filteredTweetsJson.Count + randomTweetsJson.Count < Settings.TweetParser_BatchSize)
                        Thread.Sleep(30000 - runtime);
                }
            }
            catch (Exception e)
            {
                Output.Print(Name, e);
            }
        }

        private void StoreTweetCountPerHour(List<Hashtable> filteredTweets)
        {
            if (filteredTweets.Count == 0)
                return;

            Dictionary<DateTime, int> dateHourTweetCount = new Dictionary<DateTime, int>();
            foreach (var tweet in filteredTweets)
            {
                DateTime t = (DateTime)tweet["created_at_datetime"];
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
                if (dateHourTweetCount.ContainsKey(t))
                    dateHourTweetCount[t]++;
                else
                    dateHourTweetCount.Add(t, 1);
            }

            Helpers.RunSqlStatement(Name,
                "insert into HourStatistics (DateHour, TweetsProcessed) values ("
                + string.Join("),(", dateHourTweetCount.Select(n => "'" + n.Key.ToString("yyyy-MM-dd HH:mm:ss") + "'," + n.Value.ToString()).ToArray())
                + @")
                on duplicate key update TweetsProcessed = TweetsProcessed + values(TweetsProcessed);", false);
        }

        void PerformMaintenance()
        {
            if ((_lastTweetTime - _lastMaintenanceTime).TotalMinutes >= 10)
            {
                _lastMaintenanceTime = _lastTweetTime;
                PeriodicMaintenance.Run();
            }
        }

        List<TrackFilter> GetActiveTrackFilters()
        {
            string sql = "select ID, FilterType, IsStrong, Word, UserID, Region from TwitterTrackFilter where IsActive;";

            List<TrackFilter> activeFilters = new List<TrackFilter>();
            Helpers.RunSelect(Name, sql, activeFilters, (values, reader) =>
            {
                TrackFilter filter = new TrackFilter();
                try
                {
                    filter.ID = reader.GetInt32("ID");
                    filter.Type = (TrackFilter.FilterType)reader.GetByte("FilterType");
                    filter.IsStrong = reader.GetBoolean("IsStrong");
                    if (filter.Type == TrackFilter.FilterType.User)
                    {
                        filter.UserID = reader.GetInt64("UserID");
                    }
                    else if (filter.Type == TrackFilter.FilterType.Region)
                    {
                        string[] coordinates = reader.GetString("Region").Split(',');
                        filter.Longitude1 = double.Parse(coordinates[0], CultureInfo.InvariantCulture);
                        filter.Latitude1 = double.Parse(coordinates[1], CultureInfo.InvariantCulture);
                        filter.Longitude2 = double.Parse(coordinates[2], CultureInfo.InvariantCulture);
                        filter.Latitude2 = double.Parse(coordinates[3], CultureInfo.InvariantCulture);
                    }
                    else //Word
                    {
                        filter.Word = reader.GetString("Word");
                    }
                    values.Add(filter);
                }
                catch (Exception) { }
            });

            return activeFilters;
        }

        void GetTweets(Dictionary<Int64, string> parseTweets, Dictionary<Int64, string> wordStatTweets)
        {
            string query = "select ID, WordStatsOnly, Json from TweetJson order by ID limit " + Settings.TweetParser_BatchSize + ";";

            Helpers.RunSelect<Dictionary<Int64, string>>(Name, query, null,
                (dummy, reader) =>
                {
                    if (reader.GetBoolean("WordStatsOnly"))
                        wordStatTweets.Add(reader.GetInt64("ID"), Helpers.DecodeEncodedNonAsciiCharacters(reader.GetString("Json")));
                    else
                        parseTweets.Add(reader.GetInt64("ID"), Helpers.DecodeEncodedNonAsciiCharacters(reader.GetString("Json")));
                });
        }

        List<Hashtable> ParseJsonTweets(Dictionary<Int64, string> jsonTweets, bool onlyExtractWords)
        {
            if (onlyExtractWords)
            {
                List<Hashtable> tweetTexts = new List<Hashtable>();
                foreach (var jsonTweet in jsonTweets)
                {
                    Hashtable tweet = new Hashtable();
                    int from = jsonTweet.Value.IndexOf("\"text\":\"") + "\"text\":\"".Length;
                    if (from == -1 || from >= jsonTweet.Value.Length)
                        continue;
                    int to = jsonTweet.Value.IndexOf("\",\"", from);
                    if (to <= from)
                        continue;
                    
                    tweet["text"] = jsonTweet.Value.Substring(from, to - from);
                    tweetTexts.Add(tweet);
                }
                return tweetTexts;
            }

            //Else proceed like normal and parse the whole tweet
            List<Hashtable> parsedTweets = new List<Hashtable>();

            foreach (var jsonTweet in jsonTweets)
            {
                if (jsonTweet.Value == null || jsonTweet.Value == "")
                    continue;

                //Parse JSON
                Hashtable status;
                try
                {
                    status = JSON.JsonDecode(jsonTweet.Value) as Hashtable;
                }
                catch (ArgumentOutOfRangeException) //non-interesting charsets
                {
                    continue;
                }
                catch (Exception e)
                {
                    Output.Print(Name, "Error parsing JSON: " + jsonTweet.Value);
                    Output.Print(Name, e);
                    continue; //Some parsing error occured, proceed with next message
                }

                //Is it a status message?
                if (status == null)
                {
                    Output.Print(Name, "Unknown parsing error.");
                    Output.Print(Name, jsonTweet.Value);
                    continue;
                }
                else if (status.ContainsKey("delete") || status.ContainsKey("scrub_geo")) //ignore delete and scrub_geo messages
                {
                    continue;
                }
                else if (!status.ContainsKey("text")) //ignore non-status messages
                {
                    Output.Print(Name, "Odd message: " + jsonTweet.Value);
                    continue;
                }
                //else if (status.ContainsKey("user")) //ignore non-English messages
                //{
                //    Hashtable user = (Hashtable)status["user"];
                //    if (user.ContainsKey("lang") && !user["lang"].Equals("en"))
                //        continue;
                //}

                //status.Add("sourceid", jsonTweet.Value.Value2);
                status.Add("created_at_datetime", ParseTwitterTime(status["created_at"].ToString()));
                status.Add("json_id", jsonTweet.Key);
                parsedTweets.Add(status);
            }

            return parsedTweets;
        }

        void ExtractWords(List<Hashtable> tweets, WordCount wc)
        {
            foreach (Hashtable tweet in tweets)
            {
                string text = tweet["text"] as string;
                if (text == null || text == "")
                    continue;

                text = Helpers.DecodeEncodedNonAsciiCharacters(text);
                string[] wordsInTweet = WordCount.GetWordsInString(text, useStemming: true);
                if (wordsInTweet.Length == 0)
                    continue;

                string[] uniqueWordsArr = wordsInTweet.Distinct().ToArray();
                tweet.Add("words", uniqueWordsArr);

                wc.AddWords(uniqueWordsArr);
            }

            //Word co-occurrence
            /*
             * Remove stopwords (maybe do on insert?)
             * Record all co-occurrences
             * 
             * 
             */
        }

        void ExtractUrls(List<Hashtable> tweets)
        {
            foreach (Hashtable tweet in tweets)
            {
                string text = tweet["text"] as string;
                if (text == null || text == "")
                    continue;

                List<string> urls = new List<string>();
                if (tweet.ContainsKey("entities")
                    && tweet["entities"] is Hashtable
                    && ((Hashtable)tweet["entities"]).ContainsKey("media")
                    && ((Hashtable)tweet["entities"])["media"] is ArrayList)
                {
                    ArrayList list = ((Hashtable)tweet["entities"])["media"] as ArrayList;
                    foreach (var mediaItem in list)
                    {
                        if (mediaItem is Hashtable && ((Hashtable)mediaItem).ContainsKey("expanded_url"))
                        {
                            string url = Convert.ToString(((Hashtable)mediaItem)["expanded_url"]);
                            urls.Add(url);
                        }
                    }
                }
                if (tweet.ContainsKey("entities")
                    && tweet["entities"] is Hashtable
                    && ((Hashtable)tweet["entities"]).ContainsKey("urls")
                    && ((Hashtable)tweet["entities"])["urls"] is ArrayList)
                {
                    ArrayList list = ((Hashtable)tweet["entities"])["urls"] as ArrayList;
                    foreach (var urlItem in list)
                    {
                        if (urlItem is Hashtable && ((Hashtable)urlItem).ContainsKey("expanded_url"))
                        {
                            string url = Convert.ToString(((Hashtable)urlItem)["expanded_url"]);
                            urls.Add(url);
                        }
                    }
                }

                if (urls.Count > 0)
                    tweet.Add("urls", urls.Distinct().ToArray());
            }
        }

        Dictionary<string, Word> InsertWords(WordCount wc)
        {
            //Get unique words
            IEnumerable<string> uniqueWords = wc.GetWords();
            if (uniqueWords == null || uniqueWords.Count() == 0)
                return new Dictionary<string, Word>();

            //Insert new words into Word (insert all, skip if exists)
            MySqlCommand insertCommand = new MySqlCommand();

            StringBuilder insertSql = new StringBuilder();
            insertSql.Append("INSERT IGNORE INTO Word (Word) VALUES ");
            int i = 0;
            foreach (string word in uniqueWords)
            {
                if (i > 0)
                    insertSql.Append(",");
                string varName = "@w" + i;
                insertSql.Append("(");
                insertSql.Append(varName);
                insertSql.Append(")");
                insertCommand.Parameters.AddWithValue(varName, word);
                i++;
            }
            insertSql.AppendLine(";");

            Console.Write("'");

            insertCommand.CommandText = insertSql.ToString();
            Helpers.RunSqlStatement(Name, insertCommand, false);

            Console.Write("'");

            //Get WordID for each word
            //Track keywords may be counted as stopwords if they have been stemmed before insert into WordTweet
            MySqlCommand selectCommand = new MySqlCommand();
            StringBuilder selectSql = new StringBuilder();
            selectSql.Append(
                @"SELECT
                    Word,
                    Word.WordID
                FROM Word left join WordScore on WordScore.WordID = Word.WordID WHERE Word IN (");
            AppendList(selectSql, selectCommand, uniqueWords.Cast<object>().ToArray());
            selectSql.Append(");");
            selectCommand.CommandText = selectSql.ToString();

            Dictionary<string, Word> wordIDs = new Dictionary<string, Word>();
            Helpers.RunSelect(Name, selectCommand, wordIDs,
                (values, reader) => values.Add(reader.GetString("Word"), new Word()
                {
                    ID = reader.GetInt64("WordID")
                }));

            Console.Write("'");

            if (wordIDs.Count == 0)
                return wordIDs; //Empty collection

            //Do insert-update into WordScore
            StringBuilder sbWordScore = new StringBuilder();
            sbWordScore.Append("INSERT INTO WordScore (WordID, Score1h, Score4d) VALUES ");

            bool first = true;
            //Some UTF16 characters don't end up in wordIDs
            foreach (var wordCount in wc.GetWordCounts().Where(n => wordIDs.ContainsKey(n.Key)))
            {
                if (first)
                    first = false;
                else
                    sbWordScore.Append(',');

                sbWordScore.Append('(');
                sbWordScore.Append(wordIDs[wordCount.Key].ID);
                sbWordScore.Append(',');
                sbWordScore.Append(wordCount.Value);
                sbWordScore.Append(',');
                sbWordScore.Append(wordCount.Value);
                sbWordScore.Append(')');
            }
            sbWordScore.Append(" ON DUPLICATE KEY UPDATE Score1h = Score1h + VALUES(Score1h), Score4d = Score4d + VALUES(Score4d);");

            Helpers.RunSqlStatement(Name, sbWordScore.ToString(), false);

            Console.Write("'");

            //Get wordscores and stopword status for words. Stopwords are never inserted into WordTweet.
            Dictionary<long, Word> wordIDsByID = new Dictionary<long, Word>();
            foreach (Word word in wordIDs.Values)
                wordIDsByID.Add(word.ID, word);

            string getScoresSql =
                @"SELECT 
                    WordID,
                    ScoreToIdf(Score4d) as Idf,
                    coalesce(
                        if(Word like '#%', 0, null),
                        (select 0 from TwitterTrackFilter where FilterType = 0 and Word = Word.Word and IsActive limit 1),
                        Score4d > (select value from Constants where name = 'WordScore4dHigh')
                    ) as IsStopWord
                FROM WordScore ws natural join Word WHERE WordID IN (" + string.Join(",", wordIDsByID.Keys.Select(n => n.ToString()).ToArray()) + ");";
            Helpers.RunSelect(Name, getScoresSql, wordIDsByID,
                (values, reader) =>
                {
                    Word word = values[reader.GetInt64("WordID")];
                    word.Idf = reader.GetDouble("Idf");
                    word.IsStopWord = reader.GetBoolean("IsStopword");
                });

            return wordIDs;
        }

        Dictionary<TrackFilter, FilterPerformance> RemoveOffTopicTweets(List<Hashtable> tweets, List<TrackFilter> filters)
        {
            List<Hashtable> offTopicTweets = new List<Hashtable>();
            Dictionary<TrackFilter, FilterPerformance> filterPerf = new Dictionary<TrackFilter, FilterPerformance>();
            foreach (var filter in filters)
                filterPerf.Add(filter, new FilterPerformance());

            foreach (Hashtable tweet in tweets)
            {
                //Extract words, userID, long, lat
                string[] words = (string[])tweet["words"];
                long userID = Convert.ToInt64(((Hashtable)tweet["user"])["id"]);
                double? longitude = null, latitude = null;
                if (tweet.ContainsKey("geo") && tweet["geo"] is Hashtable)
                {
                    Hashtable geoH = (Hashtable)tweet["geo"];
                    if (geoH.ContainsKey("coordinates"))
                    {
                        ArrayList coords = (ArrayList)geoH["coordinates"];
                        //Coordinates in tweet are reversed (lat, long)
                        latitude = Convert.ToDouble(coords[0], CultureInfo.InvariantCulture);
                        longitude = Convert.ToDouble(coords[1], CultureInfo.InvariantCulture);
                        tweet.Add("latitude", latitude);
                        tweet.Add("longitude", longitude);
                    }
                }

                //Check if the tweet matches any filter
                int score = 0;
                List<TrackFilter> matchedFilters = new List<TrackFilter>();
                foreach (var filter in filters)
                {
                    if (filter.Match(words, userID, longitude, latitude))
                    {
                        score += (filter.IsStrong.Value ? 2 : 1);
                        filterPerf[filter].Matches++;
                        matchedFilters.Add(filter);
                    }
                    //This speeds up processing, but breaks the filter stats
                    //if (score >= 2)
                    //    break;
                }

                if (score < 2)
                {
                    offTopicTweets.Add(tweet);
                    foreach (var filter in matchedFilters)
                        filterPerf[filter].Discards++;
                }
            }

            tweets.RemoveAll(n => offTopicTweets.Contains(n));

            return filterPerf;
        }

        void InsertFilterPerformance(Dictionary<TrackFilter, FilterPerformance> filterPerf)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("INSERT INTO TwitterTrackFilter (ID, Hits1d, Discards1d) VALUES ");

            bool first = true;
            foreach (var perf in filterPerf)
            {
                if (first)
                    first = false;
                else
                    sb.AppendLine(",");
                sb.Append("(" + perf.Key.ID + "," + perf.Value.Matches + "," + perf.Value.Discards + ")");
            }
            sb.AppendLine("ON DUPLICATE KEY UPDATE Hits1d=Hits1d+VALUES(Hits1d), Discards1d=Discards1d+VALUES(Discards1d);");
            
            Helpers.RunSqlStatement(Name, sb.ToString());
        }

        MySqlCommand BuildInsertSql(List<Hashtable> tweets, Dictionary<string, Word> words)
        {
            //Insert into Tweet, TwitterUser, TweetUrl and WordTweet

            MySqlCommand command = new MySqlCommand();

            StringBuilder sbTweet = new StringBuilder();
            StringBuilder sbWordTweet = new StringBuilder();
            StringBuilder sbTweetUrl = new StringBuilder();
            StringBuilder sbTwitterUser = new StringBuilder();

            sbTweet.Append("INSERT IGNORE INTO Tweet (TweetID, UserID, CreatedAt, WordScore, HasUrl, RetweetOf, Text, Longitude, Latitude) VALUES ");
            sbWordTweet.Append("INSERT IGNORE INTO WordTweet (WordID, TweetID) VALUES ");
            sbTweetUrl.Append("INSERT IGNORE INTO TweetUrl (TweetID, Url) VALUES ");
            sbTwitterUser.Append("INSERT INTO TwitterUser (UserID, ScreenName, RealName, ProfileImageUrl) VALUES ");

            bool hasTweets = false;
            bool hasWords = false;
            bool hasUrls = false;

            bool firstTweet = true;
            bool firstWord = true;
            bool firstUrl = true;

            foreach (Hashtable t in tweets)
            {
                try
                {
                    //Has some random bug occured during parsing?
                    if (!(t.ContainsKey("words") && t["words"] is string[]) || !t.ContainsKey("id") || !t.ContainsKey("text") || !t.ContainsKey("user") || (string)t["text"] == "")
                        continue;

                    long tweetID = Convert.ToInt64(t["id"]);
                    string[] tWords = t["words"] as string[];
                    if (tWords.Length == 0)
                        continue;

                    //Does the tweet contain any words other than stopwords, and are they specific enough?
                    double tweetWordScore = tWords.Sum(n => !words.ContainsKey(n) || words[n].IsStopWord ? 0 : words[n].Idf);
                    int tweetwordCount = tWords.Where(n => words.ContainsKey(n) && !words[n].IsStopWord).Count();
                    if (tweetWordScore <= Settings.TweetParser_MinTweetVectorLength
                        && tweetwordCount <= Settings.TweetParser_MinTweetWordCount)
                    {
                        continue;
                    }
                    hasWords = true;

                    //Get values to insert
                    Hashtable user = (Hashtable)t["user"];
                    long userID = Convert.ToInt64(user["id"]);
                    string userRealName = (string)user["name"];
                    string userScreenName = (string)user["screen_name"];
                    string userImageUrl = (string)user["profile_image_url"];
                    double? longitude = t.ContainsKey("longitude") ? (double)t["longitude"] : (double?)null;
                    double? latitude = t.ContainsKey("latitude") ? (double)t["latitude"] : (double?)null;
                    long? retweetOf = null;
                    if (t.ContainsKey("retweeted_status") && ((Hashtable)t["retweeted_status"]).ContainsKey("id_str"))
                        retweetOf = Convert.ToInt64(((Hashtable)t["retweeted_status"])["id_str"]);

                    //Is this the first tweet being processed?
                    if (firstTweet)
                        firstTweet = false;
                    else
                    {
                        sbTweet.AppendLine(",");
                        sbTwitterUser.AppendLine(",");
                    }

                    //Insert into Tweet and TwitterUser
                    AppendTuple(sbTweet, command,
                        tweetID,
                        userID,
                        (DateTime)t["created_at_datetime"],
                        tweetWordScore,
                        t.ContainsKey("urls"),
                        (retweetOf.HasValue ? retweetOf.Value : (long?)null),
                        t["text"],
                        (longitude.HasValue ? longitude.Value : (double?)null),
                        (longitude.HasValue ? latitude.Value : (double?)null)
                    );
                    AppendTuple(sbTwitterUser, command,
                        userID,
                        userScreenName,
                        userRealName,
                        userImageUrl
                    );

                    //Insert into WordTweet
                    foreach (string word in tWords.Where(n => words.ContainsKey(n) && !words[n].IsStopWord))
                    {
                        if (firstWord) firstWord = false;
                        else sbWordTweet.AppendLine(",");

                        sbWordTweet.Append("(" + words[word].ID + "," + tweetID + ")");
                    }

                    //Insert into TweetUrl
                    if (t.ContainsKey("urls") && t["urls"] is string[])
                    {
                        hasUrls = true;
                        foreach (string url in (string[])t["urls"])
                        {
                            if (firstUrl) firstUrl = false;
                            else sbTweetUrl.AppendLine(",");

                            sbTweetUrl.Append("(" + tweetID + ",'" + Helpers.EscapeSqlString(url) + "')");
                        }
                    }
                }
                catch (Exception e)
                {
                    Output.Print(Name, "Exception while building big insert statement.");
                    Output.Print(Name, e);
                }

                hasTweets = true;
            }

            if (!(hasTweets && hasWords))
                return null;

            sbTwitterUser.Append(" ON DUPLICATE KEY UPDATE ScreenName=VALUES(ScreenName), RealName=VALUES(RealName), ProfileImageUrl=VALUES(ProfileImageUrl), Score12h=Score12h+1");

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("start transaction;");
            sql.Append(sbTweet.ToString());
            sql.AppendLine(";");
            sql.Append(sbWordTweet.ToString());
            sql.AppendLine(";");
            sql.Append(sbTwitterUser.ToString());
            sql.AppendLine(";");
            if (hasUrls)
            {
                sql.Append(sbTweetUrl.ToString());
                sql.AppendLine(";");
            }
            //End by calculating total word scores of newly inserted Tweets.
            //sql.AppendLine("update Tweet t set WordScore = (select sum(ScoreToIdf(Score4d)) from WordScore natural join WordTweet where TweetID = t.TweetID) where WordScore is null;");
            sql.AppendLine("commit;");
            command.CommandText = sql.ToString();

            return command;
        }

        void AppendTuple(StringBuilder sb, MySqlCommand command, params object[] values)
        {
            sb.Append('(');
            AppendList(sb, command, values);
            sb.Append(')');
        }

        int _paramCounter = 0;
        private void AppendList(StringBuilder sb, MySqlCommand command, object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string paramName = "@p" + _paramCounter;

                if (i > 0)
                    sb.Append(",");
                sb.Append(paramName);
                command.Parameters.AddWithValue(paramName, values[i]);

                _paramCounter++;
                _paramCounter = _paramCounter % int.MaxValue;
            }
        }

        static DateTime ParseTwitterTime(string date)
        {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture).ToUniversalTime();
        }

        int DeleteParsedJsonTweets(IEnumerable<Int64> IDs)
        {
            string sql = "delete from TweetJson where ID in (" + String.Join(",", IDs.Select(n => n.ToString()).ToArray()) + ");";
            return Helpers.RunSqlStatement(Name, sql);
        }

        #region Obsolete

        Dictionary<long, UserMention> ExtractMentions(List<Hashtable> tweets)
        {
            //Count tweets, mentions(@screen_name in text) and in_reply_to_screen_name values

            Dictionary<long, UserMention> mentions = new Dictionary<long, UserMention>();
            foreach (Hashtable tweet in tweets)
            {
                if (!tweet.ContainsKey("text"))
                    continue;

                //Check in-reply-to field
                long? replyToUserID = null;
                if (tweet.ContainsKey("in_reply_to_user_id")
                    && tweet["in_repl_to_user_id"] != null)
                {
                    replyToUserID = Convert.ToInt64(tweet["in_reply_to_user_id"]);
                    string screenName = Convert.ToString(tweet["in_reply_to_screen_name"]);

                    AddUserMention(mentions, replyToUserID.Value, screenName, null);
                }

                //Check mentions
                if (tweet.ContainsKey("entities")
                    && tweet["entities"] is Hashtable
                    && ((Hashtable)tweet["entities"]).ContainsKey("user_mentions")
                    && ((Hashtable)tweet["entities"])["user_mentions"] is ArrayList)
                {
                    ArrayList list = ((Hashtable)tweet["entities"])["user_mentions"] as ArrayList;
                    foreach (var userItem in list)
                    {
                        if (userItem is Hashtable
                            && ((Hashtable)userItem).ContainsKey("id")
                            && ((Hashtable)userItem).ContainsKey("screen_name")
                            && ((Hashtable)userItem).ContainsKey("name"))
                        {
                            long id = Convert.ToInt64(((Hashtable)userItem)["id"]);
                            if (replyToUserID == id)
                                continue;

                            string screenName = Convert.ToString(((Hashtable)userItem)["screen_name"]);
                            string realName = Convert.ToString(((Hashtable)userItem)["name"]);

                            AddUserMention(mentions, id, screenName, realName);
                        }
                    }
                }

                long? retweetOf = null;
                if (tweet.ContainsKey("retweeted_status") && ((Hashtable)tweet["retweeted_status"]).ContainsKey("id_str"))
                    retweetOf = Convert.ToInt64(((Hashtable)tweet["retweeted_status"])["id_str"]);
                if (!retweetOf.HasValue) //If it's a retweet, then the points have been awarded from mentions
                {
                    Hashtable user = (Hashtable)(tweet["user"]);
                    AddUserMention(mentions,
                        Convert.ToInt64(user["id"]),
                        user["screen_name"].ToString(),
                        user["name"].ToString());
                }
            }

            return mentions;
        }

        void AddUserMention(Dictionary<long, UserMention> mentions, long id, string screenName, string realName)
        {
            if (!mentions.ContainsKey(id))
            {
                mentions.Add(id, new UserMention()
                {
                    UserID = id,
                    ScreenName = screenName,
                    RealName = realName,
                    Count = 1
                });
            }
            else
            {
                mentions[id].Count++;
            }
        }

        void InsertUserScores(Dictionary<long, UserMention> mentions)
        {
            if (mentions.Count == 0)
                return;

            MySqlCommand command = new MySqlCommand();
            StringBuilder insertSql = new StringBuilder();

            insertSql.AppendLine("insert into TwitterUser (UserID, ScreenName, RealName, Score12h) values");
            bool first = true;
            foreach (var mention in mentions.Values)
            {
                if (first)
                    first = false;
                else
                    insertSql.AppendLine(",");

                AppendTuple(insertSql, command,
                    mention.UserID,
                    mention.ScreenName,
                    mention.RealName,
                    mention.Count);
            }
            insertSql.AppendLine();
            insertSql.AppendLine(" on duplicate key update ScreenName=coalesce(VALUES(ScreenName),ScreenName), RealName=coalesce(VALUES(RealName),RealName), Score12h=Score12h+VALUES(Score12h);");
            command.CommandText = insertSql.ToString();

            Helpers.RunSqlStatement(Name, command);
        }

        #endregion

        //void PrintJsonToFile(IEnumerable<Hashtable> tweets)
        //{
        //    TextWriter file = null;
        //    DateTime fileDate = new DateTime();

        //    foreach (Hashtable tweet in tweets.OrderBy(n => (DateTime)n["created_at_datetime"]))
        //    {
        //        DateTime time = (DateTime)tweet["created_at_datetime"];

        //        if (time.Date != fileDate)
        //        {
        //            if (file != null)
        //            {
        //                file.Close();
        //                file.Dispose();
        //            }

        //            fileDate = time.Date;
        //            file = new StreamWriter(path: "json/twitter_json_" + fileDate.ToString("yyyy-MM-dd") + ".txt", append: true);
        //        }

        //        file.WriteLine(tweet["id"] + "," + tweet["json"]);
        //    }

        //    if (file != null)
        //    {
        //        file.Close();
        //        file.Dispose();
        //    }
        //}
    }
}
