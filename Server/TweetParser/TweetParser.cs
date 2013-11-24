/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
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
using System.Text.RegularExpressions;

namespace CrisisTracker.TweetParser
{
    class TweetParser
    {
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
        const string IGNORE = "ignore";
        const string CREATED_AT_DATETIME = "created_at_datetime";

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

                    HashSet<string> stopwords = GetStopwords();

                    //Get a batch of tweets
                    Dictionary<long, string> randomTweetsJson = new Dictionary<long, string>();
                    Dictionary<long, string> filteredTweetsJson = new Dictionary<long, string>();
                    GetTweets(filteredTweetsJson, randomTweetsJson);
                    Console.Write(".");

                    //Parse the Json
                    List<Hashtable> filteredTweets = ParseJsonTweets(filteredTweetsJson, onlyExtractWords: false); //Tweets are appended with CREATED_AT_DATETIME
                    List<Hashtable> randomTweets = ParseJsonTweets(randomTweetsJson, onlyExtractWords: true); //Tweets only contain a "text" field
                    Console.Write(".");

                    //Extract (stemmed) words from tweets
                    WordCount wordCounts = new WordCount();
                    ExtractWords(randomTweets, wordCounts);
                    ExtractWords(filteredTweets, wordCounts); //Tweets are appended with a "words" string array
                    Console.Write(".");

                    //Insert words into Word and WordScores
                    Dictionary<string, long> wordIDs = null;
                    if (wordCounts.HasWords)
                        wordIDs = InsertWords(wordCounts, stopwords);
                    Console.Write(".");

                    if (filteredTweets.Count > 0)
                    {
                        //Make note of the time of the last tweet
                        _lastTweetTime = (DateTime)filteredTweets.Last()[CREATED_AT_DATETIME];

                        List<Hashtable> filteredTweetsBefore = new List<Hashtable>(filteredTweets);
                        if (Settings.TweetParser_UseSecondPassFiltering)
                        {
                            //Remove the off-topic tweets from further processing
                            var filterPerf = RemoveOffTopicTweets(filteredTweets, filters); //Tweets are appended with longitude and latitude
                            Console.Write(".");

                            //Insert filter performance
                            InsertFilterPerformance(filterPerf);
                            Console.Write(".");
                        }
                        //Count number of tweets per hour
                        StoreTweetCountPerHour(filteredTweets, filteredTweetsBefore);
                        Console.Write(".");

                        //Extract URLs
                        ExtractUrls(filteredTweets); //Tweets are appended with a "urls" string array
                        Console.Write(".");

                        //Insert into Tweet, TwitterUser, WordTweet, TweetUrl
                        MySqlCommand bigInsertCommand = BuildInsertSql(filteredTweets, wordIDs, stopwords);
                        Console.Write(".");

                        Helpers.RunSqlStatement(Name, bigInsertCommand);
                        Console.Write(".");

                        ParseAidrMetatags(filteredTweets);
                    }

                    if (filteredTweets.Count + randomTweetsJson.Count > 0)
                    {
                        DeleteParsedJsonTweets(randomTweetsJson.Keys.Union(filteredTweetsJson.Keys));
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

        private void StoreTweetCountPerHour(List<Hashtable> filteredTweetsCleaned, List<Hashtable> filteredTweetsAll)
        {
            if (filteredTweetsAll.Count == 0)
                return;

            Dictionary<DateTime, int> dateHourTweetCountAll = new Dictionary<DateTime, int>();
            foreach (var tweet in filteredTweetsAll)
            {
                DateTime t = (DateTime)tweet[CREATED_AT_DATETIME];
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
                if (dateHourTweetCountAll.ContainsKey(t))
                    dateHourTweetCountAll[t]++;
                else
                    dateHourTweetCountAll.Add(t, 1);
            }

            Dictionary<DateTime, int> dateHourTweetCountCleaned = new Dictionary<DateTime, int>();
            foreach (var tweet in filteredTweetsCleaned)
            {
                DateTime t = (DateTime)tweet[CREATED_AT_DATETIME];
                t = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0);
                if (dateHourTweetCountCleaned.ContainsKey(t))
                    dateHourTweetCountCleaned[t]++;
                else
                    dateHourTweetCountCleaned.Add(t, 1);
            }

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("insert into HourStatistics (DateHour, TweetsProcessed, TweetsDiscarded) values ");
            bool first = true;
            foreach (var item in dateHourTweetCountAll)
	        {
                if (first)
                    first = false;
                else
                    sql.Append(",");

                string dateHour = item.Key.ToString("yyyy-MM-dd HH:mm:ss");
                int tweetsKept = 0;
                dateHourTweetCountCleaned.TryGetValue(item.Key, out tweetsKept);
                int tweetsDiscarded = item.Value - tweetsKept;
                sql.Append("('");
                sql.Append(dateHour);
                sql.Append("',");
                sql.Append(item.Value.ToString());
                sql.Append(",");
                sql.Append(tweetsDiscarded.ToString());
                sql.AppendLine(")");
	        }
            sql.AppendLine("on duplicate key update TweetsProcessed = TweetsProcessed + values(TweetsProcessed), TweetsDiscarded = TweetsDiscarded + values(TweetsDiscarded);");

            Helpers.RunSqlStatement(Name, sql.ToString(), false);
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
                        filter.SetWords(reader.GetString("Word"));
                    }
                    values.Add(filter);
                }
                catch (Exception) { }
            });

            return activeFilters;
        }

        HashSet<string> GetStopwords()
        {
            HashSet<string> stopwords = new HashSet<string>();
            string query = "select Word.Word from Word natural join WordScore left join TwitterTrackFilter f on Word.Word=f.Word where Score4d > (select value from Constants where name = 'WordScore4dHigh') and f.Word is null order by Score4d desc";
            Helpers.RunSelect(Name, query, stopwords, (sw, reader) => { sw.Add(reader.GetString("Word")); });
            return stopwords;
        }

        void GetTweets(Dictionary<Int64, string> parseTweets, Dictionary<Int64, string> wordStatTweets)
        {
            int batchSize = Settings.TweetParser_BatchSize;
            string query = "select ID, WordStatsOnly, Json from TweetJson order by ID limit " + batchSize + ";";

            Helpers.RunSelect<Dictionary<Int64, string>>(Name, query, null,
                (dummy, reader) =>
                {
                    if (reader.GetBoolean("WordStatsOnly"))
                        wordStatTweets.Add(reader.GetInt64("ID"), Helpers.DecodeEncodedNonAsciiCharacters(reader.GetString("Json")));
                    else
                        parseTweets.Add(reader.GetInt64("ID"), Helpers.DecodeEncodedNonAsciiCharacters(reader.GetString("Json")));
                });

            //Console.WriteLine("Fetched " + wordStatTweets.Count  + " samples and " + parseTweets.Count + " filters");
            //Console.ReadLine();
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
                    if (!jsonTweet.Value.EndsWith("}"))
                        Console.WriteLine("Incomplete tweet:\n" + jsonTweet.Value);
                    else if (jsonTweet.Value.Length > 5) //There is a strange (and known) problem with the LinqToTwitter library on Linux that adds the last few characters of each read line as a new line. Ignore these.
                        Output.Print(Name, "Unknown parsing error:\n" + jsonTweet.Value);
                    continue;
                }
                //ignore delete and scrub_geo messages
                else if (status.ContainsKey("delete") || status.ContainsKey("scrub_geo"))
                {
                    continue;
                }
                else if (!(status.ContainsKey("text") && status.ContainsKey("created_at") && status.ContainsKey("user"))) //ignore non-status messages
                {
                    Output.Print(Name, "Odd message: " + jsonTweet.Value);
                    continue;
                }

                status.Add(CREATED_AT_DATETIME, ParseTwitterTime(status["created_at"].ToString()));
                status.Add("json_id", jsonTweet.Key);

                if (status.ContainsKey("coordinates") && status["coordinates"] is Hashtable)
                {
                    Hashtable geoH = (Hashtable)status["coordinates"];
                    if (geoH.ContainsKey("coordinates"))
                    {
                        ArrayList coords = (ArrayList)geoH["coordinates"];
                        //Coordinates in tweet are [long, lat], as in the GeoJSON format
                        double longitude = Convert.ToDouble(coords[0], CultureInfo.InvariantCulture);
                        double latitude = Convert.ToDouble(coords[1], CultureInfo.InvariantCulture);
                        status.Add("longitude", longitude);
                        status.Add("latitude", latitude);
                    }
                }

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
                {
                    tweet[IGNORE] = true;
                    continue;
                }

                text = Helpers.DecodeEncodedNonAsciiCharacters(text);
                string[] wordsInTweet = WordCount.GetWordsInString(text, useStemming: true);
                //string[] wordsInTweet = WordCount.GetWordsInStringWithBigrams(text, stopwords, useStemming: true);
                if (wordsInTweet.Length == 0)
                {
                    tweet[IGNORE] = true;
                    continue;
                }

                string[] uniqueWordsArr = wordsInTweet.Distinct().ToArray();
                tweet.Add("words", uniqueWordsArr);

                bool isRetweet = text.StartsWith("RT @") || tweet.ContainsKey("retweeted_status") && ((Hashtable)tweet["retweeted_status"]).ContainsKey("id_str");
                    wc.AddWords(uniqueWordsArr, isRetweet);
            }
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

        Dictionary<string, long> InsertWords(WordCount wc, HashSet<string> stopwords)
        {
            //Get unique words
            IEnumerable<string> uniqueWords = wc.GetWords();
            if (uniqueWords == null || uniqueWords.Count() == 0)
                return new Dictionary<string, long>();

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
            MySqlCommand getAllWordsCommand = new MySqlCommand();
            StringBuilder selectSql = new StringBuilder();
            selectSql.Append(
                @"SELECT
                    Word,
                    WordID
                FROM Word WHERE Word IN (");
            Helpers.AppendList(selectSql, getAllWordsCommand, uniqueWords.Cast<object>().ToArray());
            selectSql.Append(");");
            getAllWordsCommand.CommandText = selectSql.ToString();

            Dictionary<string, long> wordIDs = new Dictionary<string, long>();
            Helpers.RunSelect(Name, getAllWordsCommand, wordIDs,
                (values, reader) => values.Add(reader.GetString("Word"), reader.GetInt64("WordID") ));

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
                sbWordScore.Append(wordIDs[wordCount.Key]);
                sbWordScore.Append(',');
                sbWordScore.Append(wordCount.Value);
                sbWordScore.Append(',');
                sbWordScore.Append(wordCount.Value);
                sbWordScore.Append(')');
            }
            sbWordScore.Append(" ON DUPLICATE KEY UPDATE Score1h = Score1h + VALUES(Score1h), Score4d = Score4d + VALUES(Score4d);");

            Helpers.RunSqlStatement(Name, sbWordScore.ToString(), false);

            Console.Write("'");

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
                long userID = Convert.ToInt64(((Hashtable)tweet["user"])["id_str"]);
                double? longitude = null, latitude = null;
                if (tweet.ContainsKey("latitude"))
                    latitude = (double)tweet["latitude"];
                if (tweet.ContainsKey("longitude"))
                    longitude = (double)tweet["longitude"];

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

        static Regex _retweetNormalizationPattern = new Regex("^RT @[a-zA-Z0-9_]+: ");
        MySqlCommand BuildInsertSql(List<Hashtable> tweets, Dictionary<string, long> words, HashSet<string> stopwords)
        {
            /*TODO: Somewhere there is a bug that inserts the wrong tweet ID with the wrong content. */

            //Insert into Tweet, TwitterUser, TweetUrl and WordTweet

            MySqlCommand command = new MySqlCommand();

            StringBuilder sbTweet = new StringBuilder();
            StringBuilder sbWordTweet = new StringBuilder();
            StringBuilder sbTweetUrl = new StringBuilder();
            StringBuilder sbTwitterUser = new StringBuilder();

            sbTweet.Append("INSERT IGNORE INTO Tweet (TweetID, UserID, CreatedAt, RetweetOf, RetweetOfUserID, Text, TextHash, Longitude, Latitude, Language, ProcessedMetatags) VALUES ");
            sbWordTweet.Append("INSERT IGNORE INTO WordTweet (WordID, TweetID) VALUES ");
            sbTweetUrl.Append("INSERT IGNORE INTO TweetUrl (TweetID, UrlHash, Url) VALUES ");
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
                    if (!(t.ContainsKey("words") && t["words"] is string[]) || !t.ContainsKey("id_str") || !t.ContainsKey("text") || !t.ContainsKey("user") || (string)t["text"] == "")
                    {
                        t[IGNORE] = true;
                        continue;
                    }

                    long tweetID = Convert.ToInt64(t["id_str"]);
                    string[] tWords = t["words"] as string[];
                    if (tWords.Length == 0)
                    {
                        t[IGNORE] = true;
                        continue;
                    }

                    //Does the tweet contain enough non-stopwords?
                    int tweetwordCount = tWords.Where(n => words.ContainsKey(n) && !stopwords.Contains(n)).Count();
                    if (tweetwordCount < Settings.TweetParser_MinTweetWordCount)
                    {
                        t[IGNORE] = true;
                        Console.WriteLine(t["text"]);
                        continue;
                    }
                    hasWords = true;

                    //Get values to insert
                    Hashtable user = (Hashtable)t["user"];
                    long userID = Convert.ToInt64(user["id_str"]);
                    string userRealName = (string)user["name"];
                    string userScreenName = (string)user["screen_name"];
                    string userImageUrl = (string)user["profile_image_url"];
                    double? longitude = t.ContainsKey("longitude") ? (double)t["longitude"] : (double?)null;
                    double? latitude = t.ContainsKey("latitude") ? (double)t["latitude"] : (double?)null;
                    string language = t.ContainsKey("lang") ? (string)t["lang"] : "und";
                    long? retweetOf = null, retweetOfUserID = null;
                    if (t.ContainsKey("retweeted_status") && ((Hashtable)t["retweeted_status"]).ContainsKey("id_str"))
                    {
                        var sourcetweet = (Hashtable)t["retweeted_status"];
                        retweetOf = Convert.ToInt64(sourcetweet["id_str"]);
                        retweetOfUserID = Convert.ToInt64(((Hashtable)sourcetweet["user"])["id_str"]);
                    }

                    //Is this the first tweet being processed?
                    if (firstTweet)
                        firstTweet = false;
                    else
                    {
                        sbTweet.AppendLine(",");
                        sbTwitterUser.AppendLine(",");
                    }

                    //Insert into Tweet and TwitterUser
                    string normText = _retweetNormalizationPattern.Replace((string)t["text"], "");
                    Helpers.AppendTuple(sbTweet, command,
                        tweetID,
                        userID,
                        (DateTime)t[CREATED_AT_DATETIME],
                        (retweetOf.HasValue ? retweetOf.Value : (long?)null),
                        (retweetOfUserID.HasValue ? retweetOfUserID.Value : (long?)null),
                        t["text"],
                        normText.Substring(0, Math.Min(normText.Length, 40)).GetHashCode(),
                        (longitude.HasValue ? longitude.Value : (double?)null),
                        (longitude.HasValue ? latitude.Value : (double?)null),
                        language,
                        1
                    );
                    Helpers.AppendTuple(sbTwitterUser, command,
                        userID,
                        userScreenName,
                        userRealName,
                        userImageUrl
                    );

                    //Insert into WordTweet
                    foreach (string word in tWords.Where(n => words.ContainsKey(n) && !stopwords.Contains(n)))
                    {
                        if (firstWord) firstWord = false;
                        else sbWordTweet.AppendLine(",");

                        sbWordTweet.Append("(" + words[word] + "," + tweetID + ")");
                    }

                    //Insert into TweetUrl
                    if (t.ContainsKey("urls") && t["urls"] is string[])
                    {
                        hasUrls = true;
                        foreach (string url in (string[])t["urls"])
                        {
                            if (firstUrl) firstUrl = false;
                            else sbTweetUrl.AppendLine(",");

                            sbTweetUrl.Append("(" + tweetID + "," + url.GetHashCode() + ",'" + Helpers.EscapeSqlString(url) + "')");
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
            sql.AppendLine("commit;");
            command.CommandText = sql.ToString();

            return command;
        }

        static DateTime ParseTwitterTime(string date)
        {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture).ToUniversalTime();
        }

        bool tweetHasAidrTags(Hashtable tweet)
        {
            return tweet.ContainsKey("aidr") 
                && ((Hashtable)tweet["aidr"]).ContainsKey("nominal_labels")
                && ((Hashtable)tweet["aidr"])["nominal_labels"] is ArrayList
                && ((ArrayList)((Hashtable)tweet["aidr"])["nominal_labels"]).Count > 0;
        }

        void ParseAidrMetatags(List<Hashtable> tweets)
        {
            var hasNominalLabels = tweets.Where(n => !n.ContainsKey(IGNORE) && tweetHasAidrTags(n));
            if (!hasNominalLabels.Any())
                return;

            Dictionary<long, List<AidrLabel>> tweetLabels = new Dictionary<long, List<AidrLabel>>();
            foreach (var item in hasNominalLabels)
            {
                long tweetID = Convert.ToInt64(item["id_str"]);
                Hashtable aidr = (Hashtable)item["aidr"];
                ArrayList nominalLabels = (ArrayList)aidr["nominal_labels"];
                foreach (Hashtable labelData in nominalLabels)
                {
                    AidrLabel label = new AidrLabel();
                    label.AttributeCode = labelData["attribute_code"].ToString();
                    label.AttributeName = labelData["attribute_name"].ToString();
                    label.LabelCode = labelData["label_code"].ToString();
                    label.LabelName = labelData["label_name"].ToString();
                    label.Confidence = Convert.ToDouble(labelData["confidence"], CultureInfo.InvariantCulture);

                    //Don't store null tags or tags with low confidence
                    if (label.LabelCode == "null" || label.Confidence < Settings.TweetParser_MinAidrLabelConficence)
                        continue;

                    if (!tweetLabels.ContainsKey(tweetID))
                        tweetLabels.Add(tweetID, new List<AidrLabel>());
                    tweetLabels[tweetID].Add(label);
                }
            }

            if (!tweetLabels.Any())
                return;

            //Group all tweetLabels into an attribute->label structure
            var attributeDefinitions = 
                from item in tweetLabels.Values.SelectMany(n => n)
                group item by item.AttributeCode into attributeGroup
                select new { 
                    AttributeCode = attributeGroup.Key, 
                    AttributeName = attributeGroup.First().AttributeName, 
                    Labels = from label in attributeGroup.ToList()
                            group label by label.LabelCode into labelGroup
                            select new {
                                LabelCode = labelGroup.Key,
                                LabelName = labelGroup.First().LabelName
                            }
                    };

            //Insert attribute definitions
            StringBuilder sbAttr = new StringBuilder();
            sbAttr.Append("insert into AidrAttribute (AttributeCode, AttributeName) values ");
            bool firstLine = true;
            foreach (var attribute in attributeDefinitions) 
            {
                if (firstLine)
                    firstLine = false;
                else
                    sbAttr.Append(",");

                sbAttr.Append("('");
                sbAttr.Append(attribute.AttributeCode);
                sbAttr.Append("','");
                sbAttr.Append(attribute.AttributeName);
                sbAttr.Append("')");
            }
            sbAttr.Append(" on duplicate key update AttributeName=values(AttributeName)");
            Helpers.RunSqlStatement(Name, sbAttr.ToString(), false);

            //Get attribute IDs
            //string attributeCodesStr = string.Join("','", attributeDefinitions.Select(n => n.AttributeCode).ToArray());
            Dictionary<string, uint> attributeIDs = new Dictionary<string,uint>();
            Helpers.RunSelect(Name, 
                "select AttributeID, AttributeCode from AidrAttribute", 
                attributeIDs,
                (values, reader) => attributeIDs.Add(reader.GetString("AttributeCode"), reader.GetUInt32("AttributeID")));

            //Insert label definitions
            var labelDefinitions =
                from attrDef in attributeDefinitions
                from lblDef in attrDef.Labels
                select new
                {
                    AttributeID = attributeIDs[attrDef.AttributeCode],
                    LabelCode = lblDef.LabelCode,
                    LabelName = lblDef.LabelName
                };

            StringBuilder sbLabel = new StringBuilder();
            sbLabel.Append("insert into AidrLabel (AttributeID, LabelCode, LabelName) values ");
            firstLine = true;
            foreach (var label in labelDefinitions)
            {
                if (firstLine)
                    firstLine = false;
                else
                    sbLabel.Append(",");

                sbLabel.Append("(");
                sbLabel.Append(label.AttributeID);
                sbLabel.Append(",'");
                sbLabel.Append(label.LabelCode);
                sbLabel.Append("','");
                sbLabel.Append(label.LabelName);
                sbLabel.Append("')");
            }
            sbLabel.Append(" on duplicate key update LabelName=values(LabelName)");
            Helpers.RunSqlStatement(Name, sbLabel.ToString(), false);

            //Get IDs for labels
            Dictionary<uint, Dictionary<string, uint>> labelIDs = new Dictionary<uint, Dictionary<string, uint>>();
            Helpers.RunSelect(Name, 
                "select AttributeID, LabelCode, LabelID from AidrLabel", 
                labelIDs,
                (values, reader) => 
                {
                    uint attrId = reader.GetUInt32("AttributeID");
                    string lblCode = reader.GetString("LabelCode");
                    uint lblId = reader.GetUInt32("LabelID");
                    if (!labelIDs.ContainsKey(attrId))
                        labelIDs.Add(attrId, new Dictionary<string,uint>());
                    labelIDs[attrId].Add(lblCode, lblId);
                });

            //Insert tweet tags
            StringBuilder sbTweetTag = new StringBuilder();
            sbTweetTag.Append("insert ignore into TweetAidrAttributeTag (TweetID, LabelID, Confidence) values ");
            firstLine = true;
            foreach (var tweet in tweetLabels)
            {
                foreach (var label in tweet.Value)
	            {
                    if (firstLine)
                        firstLine = false;
                    else
                        sbTweetTag.Append(",");

                    sbTweetTag.Append("(");
                    sbTweetTag.Append(tweet.Key);
                    sbTweetTag.Append(",");
                    sbTweetTag.Append(labelIDs[attributeIDs[label.AttributeCode]][label.LabelCode]);
                    sbTweetTag.Append(",");
                    sbTweetTag.Append(label.Confidence.ToString(CultureInfo.InvariantCulture));
                    sbTweetTag.Append(")");
	            }
            }
            Helpers.RunSqlStatement(Name, sbTweetTag.ToString(), false);

            Helpers.RunSqlStatement(Name, "update Tweet set ProcessedMetatags=0 where TweetID in (" 
                + String.Join(",", tweetLabels.Keys.Select(n => n.ToString()).ToArray()) + ")", false);
        }

        int DeleteParsedJsonTweets(IEnumerable<Int64> IDs)
        {
            string sql = "delete from TweetJson where ID in (" + String.Join(",", IDs.Select(n => n.ToString()).ToArray()) + ");";
            return Helpers.RunSqlStatement(Name, sql);
        }
    }
}
