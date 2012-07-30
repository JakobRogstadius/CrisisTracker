using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrisisTracker.Common;
using MySql.Data.MySqlClient;
using System.IO;
using CrisisTracker.TweetClusterer;

namespace CrisisTracker.FilterStreamConsumer
{
    class Program
    {
        const string _name = "EvalCoordinator";

        class Tweet
        {
            public long TweetID { get; set; }
            public DateTime CreatedAt { get; set; }
            public string[] Words { get; set; }
        }
        class Word
        {
            public long ID { get; set; }
            public bool IsStopWord { get; set; }
            public double Idf { get; set; }
        }

        static TextReader _sampleStreamReader;
        static Queue<string> _sampleStreamFiles;

        static void Main(string[] args)
        {
            string u = Settings.FilterStreamConsumer_Username;
            Test();

            try
            {
                Output.Print(_name, "Starting batch processing...");

                //Interval
                long startAtID = 143479600000000000;
                long stopAtID = 149276747383320578; //stop at the first tweet of the 2011-12-21 file
                if (args != null && args.Length == 2)
                {
                    long.TryParse(args[0], out startAtID);
                    long.TryParse(args[1], out stopAtID);
                }

                //Initialize
                long lastProcessedID = startAtID;
                long processedCount = 0;
                DateTime lastRehashTime = new DateTime();
                DateTime lastMaintenanceTime = new DateTime();
                bool initializedNeighborFinder = false;
                InitializeRandomStreamFileNames();

                TweetClusterWorker neighborFinder = new TweetClusterWorker();

                //Process tweets
                while (lastProcessedID < stopAtID)
                {
                    //Get batch of tweets + corresponding batch of random stream words
                    Dictionary<long, Tweet> tweets = GetTweetBatch(lastProcessedID, 400); //Was 200
                    Console.WriteLine("Processing " + tweets.Count + " tweets");
                    processedCount += tweets.Count;

                    //Extract words from tweets
                    Dictionary<string, int> wordCounts = new Dictionary<string, int>();
                    foreach (Tweet t in tweets.Values)
                    {
                        foreach (string word in t.Words)
                        {
                            if (wordCounts.ContainsKey(word))
                                wordCounts[word]++;
                            else
                                wordCounts.Add(word, 1);
                        }
                    }
                    int sampleStreamTweetCount = AddSampleStreamWords(wordCounts, tweets.Keys.Max(), 2000); //Was 2000
                    Console.WriteLine("Calculated word counts (" + sampleStreamTweetCount + " tweets from sample stream)");

                    //Update DB word stats
                    Dictionary<string, Word> words = InsertWords(wordCounts);
                    Console.WriteLine("Inserted " + words.Count + " words");

                    //Insert into Tweet and WordTweet
                    InsertToTweetAndWordTweet(tweets, words);
                    Console.WriteLine("Inserted tweets");

                    DateTime lastTweetTime = tweets.Values.Max(n => n.CreatedAt);
                    if (processedCount > 2000) // && (lastTweetTime - lastStoryTime).Minutes > 9)
                    {
                        if (!initializedNeighborFinder)
                        {
                            neighborFinder.CreateHashTables();
                            neighborFinder.InitializeWithOldTweets();
                            initializedNeighborFinder = true;
                        }

                        //Calculate tweet relations and TweetClusterIDs
                        while (neighborFinder.ProcessTweetBatch() > 0)
                            ;
                        Console.WriteLine("Calculated relations");

                        //Refine tweet clusters
                        //TweetClusterWorker.Run();

                        //Perform agglomerative grouping of clusters into stories
                        StoryWorker.Run();

                        //lastStoryTime = lastTweetTime;
                    }

                    //Perform maintenance of word stats
                    if ((lastTweetTime - lastMaintenanceTime).Minutes > 10)
                    {
                        Helpers.RunSqlStatement(_name, "update WordScore set Score1h = Score1h * 0.890899, Score4d = Score4d * 0.99879734;");
                        Helpers.RunSqlStatement(_name, "delete wt.* from WordScore ws, WordTweet as wt where ws.WordID = wt.WordID and (score4d < 50 and score1h < 0.5);");
                        Helpers.RunSqlStatement(_name, "delete w.*, ws.* from WordScore ws, Word as w where ws.WordID = w.WordID and (score4d < 50 and score1h < 0.5);");
                        Helpers.RunSqlStatement(_name, "update Constants set value = (select 0.1 * max(score4d) from WordScore) where name = 'WordScore4dHigh';");
                        Helpers.RunSqlStatement(_name,
                            @"delete r.* from TweetRelation r natural join 
                            (
                                select TweetID1, TweetID2, tc1.IsArchived or tc2.IsArchived as IsArchived
                                from TweetRelation r
                                join Tweet t1 on t1.TweetID=r.TweetID1
                                join TweetCluster tc1 on tc1.TweetClusterID=t1.TweetClusterID
                                join Tweet t2 on t2.TweetID=r.TweetID2
                                join TweetCluster tc2 on tc2.TweetClusterID=t2.TweetClusterID
                            ) T
                            where IsArchived;");
                        lastMaintenanceTime = lastTweetTime;
                        Console.WriteLine("Performed maintenance");
                    }

                    //Update hashtables
                    if (initializedNeighborFinder && (lastTweetTime - lastRehashTime).Hours > 4)
                    {
                        neighborFinder.UpdateOldestHashFunction();
                        lastRehashTime = lastTweetTime;
                        Console.WriteLine("Recalculated a hash function");
                    }

                    lastProcessedID = tweets.Keys.Max();

                    //Console.ReadLine();
                }

                //Clean up
                if (_sampleStreamReader != null)
                {
                    _sampleStreamReader.Close();
                    _sampleStreamReader.Dispose();
                }

                Output.Print(_name, "Finished batch processing.");
            }
            catch (Exception e)
            {
                Output.Print(_name, e);
            }
        }

        static void Test()
        {

            //StoryWorker2.Run();

            Dictionary<long, TmpClass> testData = new Dictionary<long, TmpClass>();
            testData.Add(1, new TmpClass() { StoryID = 1, Value = 5 });
            testData.Add(2, new TmpClass() { StoryID = 2, Value = 2 });
            testData.Add(3, new TmpClass() { StoryID = 3, Value = 3 });
            testData.Add(4, new TmpClass() { StoryID = 4, Value = 7 });
            testData.Add(5, new TmpClass() { StoryID = 5, Value = 6 });
            testData.Add(6, new TmpClass() { StoryID = 6, Value = 4 });
            testData.Add(7, new TmpClass() { StoryID = 7, Value = 10 });
            testData.Add(8, new TmpClass() { StoryID = 8, Value = 8 });
            testData.Add(9, new TmpClass() { StoryID = 9, Value = 9 });

            ClusterStories(testData);
        }

        class TmpClass
        {
            public long StoryID { get; set; }
            public double Value { get; set; }
            public long? MergedWith { get; set; }
        }

        static void ClusterStories(Dictionary<long, TmpClass> stories)
        {
            //Find all edges
            //Loop through edges


            if (stories.Count < 2)
                return;

            bool changed = true;
            while (changed)
            {
                Dictionary<long, HashSet<long>> mergeActions = new Dictionary<long, HashSet<long>>();

                //Find all stories that are near each other
                foreach (var story1 in stories.Values.OrderBy(n => n.StoryID))
                {
                    var neighbors = stories.Values.Where(story2 =>
                        story2.StoryID > story1.StoryID
                        && Math.Abs(story1.Value - story2.Value) <= 1)
                        .ToList();

                    long? targetID = neighbors.Min(n => n.MergedWith);
                    if (!targetID.HasValue || targetID > story1.MergedWith)
                        targetID = story1.MergedWith;
                    if (!targetID.HasValue || targetID > story1.StoryID)
                        targetID = story1.StoryID;

                    foreach (var neighbor in neighbors)
                    {
                        if (!mergeActions.ContainsKey(targetID.Value))
                            mergeActions.Add(targetID.Value, new HashSet<long>());
                        mergeActions[targetID.Value].Add(neighbor.StoryID);
                        neighbor.MergedWith = targetID;
                    }
                }
                changed = mergeActions.Count > 0;

                //Perform story merges
                foreach (var group in mergeActions)
                {
                    long storyID = group.Key;
                    HashSet<long> members = group.Value;
                    List<TmpClass> memberStories = members.Select(n => stories[n]).ToList();
                    memberStories.Add(stories[storyID]);
                    stories[storyID].Value = memberStories.Select(n => n.Value).Average();

                    foreach (long deleteStoryID in members)
                    {
                        stories.Remove(deleteStoryID);
                    }
                }
            }


        }


        static Dictionary<long, Tweet> GetTweetBatch(long lastProcessedID, int batchSize)
        {
            Dictionary<long, Tweet> tweets = new Dictionary<long, Tweet>();

            string sql = "select TweetID, CreatedAt, Text from SyriaTweetBackup where TweetID > " + lastProcessedID + " order by TweetID limit " + batchSize + ";";
            Helpers.RunSelect(_name, sql, tweets, (values, reader) =>
                {
                    long id = reader.GetInt64("TweetID");
                    DateTime createdAt = reader.GetDateTime("CreatedAt");
                    string text = reader.GetString("Text");
                    string[] words = WordCount.GetWordsInString(text, useStemming: true);
                    values.Add(id, new Tweet()
                    {
                        TweetID = id,
                        CreatedAt = createdAt,
                        Words = words
                    });
                });

            return tweets;
        }

        static void InitializeRandomStreamFileNames()
        {
            _sampleStreamFiles = new Queue<string>();

            DateTime date = new DateTime(2011, 11, 28);
            DateTime to = new DateTime(2011, 12, 20);
            do
            {
                _sampleStreamFiles.Enqueue("samplestream/twitter_json_" + date.ToString("yyyy-MM-dd") + "_words_unicode.txt");
            } while ((date = date.AddDays(1)) != to);
        }

        static int AddSampleStreamWords(Dictionary<string, int> wordCounts, long stopAtTweetID, int maxCount)
        {
            int processedCount = 0;

            if (_sampleStreamReader == null)
            {
                if (_sampleStreamFiles.Count == 0)
                    return 0;
                _sampleStreamReader = new StreamReader(_sampleStreamFiles.Dequeue());
            }

            long tweetID = 0;
            int tweetCount = 0;
            int skippedCount = 0;
            do
            {
                string line = _sampleStreamReader.ReadLine();
                tweetCount++;

                if (line == null) //Reached end of file
                {
                    _sampleStreamReader.Close();
                    _sampleStreamReader.Dispose();
                    _sampleStreamReader = null;
                    //Open new file
                    if (_sampleStreamFiles.Count == 0)
                        return processedCount;
                    _sampleStreamReader = new StreamReader(_sampleStreamFiles.Dequeue());
                    line = _sampleStreamReader.ReadLine();
                }

                int commaIndex = line.IndexOf(',');
                tweetID = long.Parse(line.Substring(0, commaIndex));
                if (tweetCount > maxCount)
                {
                    skippedCount++;
                    continue;
                }
                commaIndex = line.IndexOf(',', commaIndex + 1); // RetweetOf
                commaIndex = line.IndexOf(',', commaIndex + 1); // CreatedAt
                string[] words = line.Substring(commaIndex + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < words.Length; i++)
                {
                    string word = WordCount.NaiveStemming(words[i]);
                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts.Add(word, 1);
                }

                processedCount++;

            } while (tweetID < stopAtTweetID);

            if (skippedCount > 0)
                Console.WriteLine("Skipped " + skippedCount + " sample stream tweets.");

            return processedCount;
        }

        static void RemoveRareWords(Dictionary<string, int> wordCounts)
        {
            var rareWords = wordCounts.Where(n => n.Value == 1).Select(n => n.Key).ToList();
            foreach (var rareWord in rareWords)
            {
                wordCounts.Remove(rareWord);
            }
        }

        static Dictionary<string, Word> InsertWords(Dictionary<string, int> wc)
        {
            //Get unique words
            if (wc.Count == 0)
                return new Dictionary<string, Word>();

            //Insert new words into Word (insert all, skip if exists)
            MySqlCommand insertCommand = new MySqlCommand();

            StringBuilder insertSql = new StringBuilder();
            insertSql.Append("INSERT IGNORE INTO Word (Word) VALUES ");
            int i = 0;
            foreach (string word in wc.Keys)
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
            Helpers.RunSqlStatement(_name, insertCommand);

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
            AppendList(selectSql, selectCommand, wc.Keys.Cast<object>().ToArray());
            selectSql.Append(");");
            selectCommand.CommandText = selectSql.ToString();

            Dictionary<string, Word> wordIDs = new Dictionary<string, Word>();
            Helpers.RunSelect(_name, selectCommand, wordIDs,
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
            foreach (var wordCount in wc.Where(n => wordIDs.ContainsKey(n.Key)))
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

            Helpers.RunSqlStatement(_name, sbWordScore.ToString());

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
            Helpers.RunSelect(_name, getScoresSql, wordIDsByID,
                (values, reader) =>
                {
                    Word word = values[reader.GetInt32("WordID")];
                    word.Idf = reader.GetDouble("Idf");
                    word.IsStopWord = reader.GetBoolean("IsStopword");
                });

            return wordIDs;
        }

        static void InsertToTweetAndWordTweet(Dictionary<long, Tweet> tweets, Dictionary<string, Word> words)
        {
            if (tweets.Count == 0)
                return;

            //Insert into Tweet
            string tweetIDsStr = string.Join(",", tweets.Keys.Select(n => n.ToString()).ToArray());
            string tweetsInsert = "INSERT IGNORE INTO Tweet (TweetID, UserID, CreatedAt, HasUrl, RetweetOf, Text, Longitude, Latitude) "
                + "select TweetID, UserID, CreatedAt, HasUrl, RetweetOf, Text, Longitude, Latitude from SyriaTweetBackup "
                + "where TweetID in (" + tweetIDsStr + ");";
            Helpers.RunSqlStatement(_name, tweetsInsert);


            //Insert into WordTweet
            MySqlCommand command = new MySqlCommand();
            StringBuilder sbWordTweet = new StringBuilder();
            sbWordTweet.Append("INSERT IGNORE INTO WordTweet (WordID, TweetID) VALUES ");

            bool firstWord = true;
            foreach (Tweet t in tweets.Values)
            {
                try
                {
                    //Insert into WordTweet
                    foreach (string word in t.Words.Where(n => words.ContainsKey(n) && !words[n].IsStopWord))
                    {
                        if (firstWord) firstWord = false;
                        else sbWordTweet.AppendLine(",");

                        sbWordTweet.Append("(" + words[word].ID + "," + t.TweetID + ")");
                    }
                }
                catch (Exception e)
                {
                    Output.Print(_name, "Exception while building big insert statement.");
                    Output.Print(_name, e);
                }
            }

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("start transaction;");
            sql.Append(sbWordTweet.ToString());
            sql.AppendLine(";");
            sql.AppendLine("commit;");
            command.CommandText = sql.ToString();

            Helpers.RunSqlStatement(_name, command);
        }

        static void AppendTuple(StringBuilder sb, MySqlCommand command, params object[] values)
        {
            sb.Append('(');
            AppendList(sb, command, values);
            sb.Append(')');
        }

        static int _paramCounter = 0;
        static void AppendList(StringBuilder sb, MySqlCommand command, object[] values)
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

        class TmpClusterVector
        {
            public long ClusterID;
            public long StoryID;
            public WordVector Vector;
        }

        static void CalculateVectorDistances()
        {
            Dictionary<int, TmpClusterVector> clusters = new Dictionary<int, TmpClusterVector>();

            using (TextReader reader = new StreamReader(@"C:\Projects\uma-svn\CrisisTracker4\data\tweet_cluster_word_vectors.csv"))
            {
                string line = reader.ReadLine(); //skip headers
                while ((line = reader.ReadLine()) != null)
                {
                    //TweetClusterID,StoryID,WordID,Word,Weight
                    string[] fields = line.Split(',');

                    int clusterID = int.Parse(fields[0]);
                    int storyID = int.Parse(fields[0]);

                    if (!clusters.ContainsKey(clusterID))
                        clusters.Add(clusterID, new TmpClusterVector() { ClusterID = clusterID, StoryID = storyID, Vector = new WordVector() });

                    clusters[clusterID].Vector.AddItem(long.Parse(fields[2]), double.Parse(fields[4], System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            foreach (var item in clusters.Values)
            {
                item.Vector.Normalize();
            }

            List<double[]> relations = new List<double[]>();
            foreach (var item1 in clusters.Values)
            {
                foreach (var item2 in clusters.Values.Where(n => n.ClusterID > item1.ClusterID))
                {
                    double similarity = item1.Vector * item2.Vector;
                    if (similarity > 0.05)
                        relations.Add(new double[] { item1.ClusterID, item2.ClusterID, 100 * similarity });
                }
            }

            using (TextWriter writer = new StreamWriter(@"C:\Projects\uma-svn\CrisisTracker4\data\tweet_cluster_distances.csv"))
            {
                writer.WriteLine("source,target,weight");
                foreach (double[] relation in relations)
                {
                    writer.WriteLine(string.Join(",", relation.Select(n => n.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)).ToArray()));
                }
            }
        }
    }
}
