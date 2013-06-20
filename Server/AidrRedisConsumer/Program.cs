using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using CrisisTracker.Common;
using System.Threading;

namespace AidrRedisConsumer
{
    class Program
    {
        static string _name = "AidrRedisConsumer";
        static DateTime _lastDBWrite = new DateTime(1000, 1, 1);
        static Queue<string> _tweetQueue;
        static BackgroundWorker _worker;
        static bool _dbFail = false;
        static int _rateLimitPerMinute = 10000;
        static object _workerLock = new object();

        //TODO: Move to settings
        class LocalSettings
        {
            public string InputChannel = "aidr-predict:syria-civil-war";
            public string RedisHost = "localhost";
            public int RedisPort = 1234;
        }

        static void Main(string[] args)
        {
            _tweetQueue = new Queue<string>();
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(DBWrite);

            DateTime executionStart = DateTime.Now;
            while (true)
            {
                Console.Write("-");
                try
                {
                    executionStart = DateTime.Now;
                    ConsumeStream();
                }
                catch (Exception e)
                {
                    Output.Print(_name, "Exception from Run:" + Environment.NewLine + e);
                    while (_worker.IsBusy)
                    {
                        Console.WriteLine("Waiting for worker to finish");
                        Thread.Sleep(1000);
                    }
                    if ((DateTime.Now - executionStart).TotalSeconds < 5)
                    {
                        Output.Print(_name, "Previous failure was quick. Waiting 60 seconds.");
                        Thread.Sleep(59000);
                    }
                }
                Console.WriteLine(".");
                Thread.Sleep(1000);
            }

        }

        static void ConsumeStream()
        {
            
            using (var redis = new RedisClient())
            {
                using (var subscription = redis.CreateSubscription())
                {
                    subscription.OnSubscribe = (channel) => {
                        Console.WriteLine("Subscribed to " + channel);
                    };

                    subscription.OnUnSubscribe = (channel) =>
                    {
                        Console.WriteLine("Unsubscribed from " + channel);
                    };

                    subscription.OnMessage = (channel, message) =>
                    {
                        Console.Write(".");
                        _tweetQueue.Enqueue(message);
                        AllowAsyncDBWrite();
                    };

                    subscription.SubscribeToChannels("aidr-predict.syria-civil-war");
                }
            }
        }

        static void AllowAsyncDBWrite()
        {
            lock (_workerLock)
            {
                if (_worker.IsBusy || _tweetQueue.Count == 0 || (_tweetQueue.Count < 100 && (DateTime.Now - _lastDBWrite).TotalSeconds < 20))
                    return;
                Console.WriteLine("Starting worker...");
                _worker.RunWorkerAsync();
            }
        }

        static void DBWrite(object sender, DoWorkEventArgs args)
        {
            Console.WriteLine("Writing to DB");

            //Check rate limit
            DateTime writeTime = DateTime.Now;
            int maxWrites = (int)Math.Ceiling(_rateLimitPerMinute * (writeTime - _lastDBWrite).TotalMinutes);
            _lastDBWrite = writeTime;

            int discardedCount = 0;
            List<string> tweets = new List<string>();
            while (_tweetQueue.Count > 0)
            {
                string tweet = _tweetQueue.Dequeue();
                if (tweets.Count < maxWrites)
                    tweets.Add(tweet);
                else
                    discardedCount++;
            }

            if (discardedCount > 0)
                Console.WriteLine("Rate limited: Discarding " + discardedCount + " tweets (saving " + tweets.Count + ")");

            if (tweets.Count == 0)
                return;

            Console.WriteLine("Writing " + tweets.Count + " tweets");

            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(Settings.ConnectionString);
                connection.Open();

                _dbFail = false;

                using (MySqlCommand command = connection.CreateCommand())
                {
                    SetInsertStatement(command, tweets);
                    try
                    {
                        Helpers.RunSqlStatement(_name, command);
                    }
                    catch (MySqlException e)
                    {
                        command.Cancel();
                        command.Dispose();
                        throw e;
                    }
                }
            }
            catch (Exception e)
            {
                if (_dbFail)
                {
                    Thread.Sleep(20000);
                }
                else
                {
                    if (e.InnerException is System.Net.Sockets.SocketException)
                    {
                        Output.Print(_name, "Failed to connect to database:" + Environment.NewLine + e);
                        _dbFail = true;
                    }
                    else
                    {
                        Output.Print(_name, "Exception when inserting tweets:" + Environment.NewLine + e);
                    }
                }
            }
            finally
            {
                try
                {
                    connection.Close();
                }
                catch (Exception e)
                {
                    Output.Print(_name, "Exception when trying to close DB connection:" + Environment.NewLine + e);
                }
            }
        }

        static void SetInsertStatement(MySqlCommand command, List<string> tweets)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO TweetJson (WordStatsOnly,Json) VALUES ");

            for (int i = 0; i < tweets.Count; i++)
            {
                string paramName = "@t" + i;
                if (i > 0)
                    sb.Append(",");
                sb.Append("(0,");
                sb.Append(paramName);
                sb.Append(')');
                command.Parameters.AddWithValue(paramName, tweets[i]);
            }
            sb.Append(";");

            command.CommandText = sb.ToString();
        }
    }
}
