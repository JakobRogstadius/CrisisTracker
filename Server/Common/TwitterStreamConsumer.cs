/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using Procurios.Public;
using System.Linq;
using LinqToTwitter;

namespace CrisisTracker.Common
{
    public abstract class TwitterStreamConsumer
    {
        DateTime _lastDBWrite = new DateTime(1000, 1, 1);
        Queue<string> _tweetQueue;
        BackgroundWorker _worker;
        string _connectionString;
        bool _wordStatsOnly;
        bool _dbFail = false;
        protected int RateLimitPerMinute = 3000;
        bool _isConsuming = false;
        TwitterContext _twitterContext = null;

        public string OutputFileDirectory { get; set; }

        public bool ResetPending { get; set; }

        public string Name { get; set; }

        protected abstract InMemoryCredentials GetCredentials();
        protected abstract IQueryable<Streaming> GetStreamQuery(TwitterContext context);

        public TwitterStreamConsumer(string connectionString, bool useForWordStatsOnly)
        {
            _tweetQueue = new Queue<string>();
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(DBWrite);

            _connectionString = connectionString;
            _wordStatsOnly = useForWordStatsOnly;
        }

        public void Run()
        {
            DateTime executionStart = DateTime.Now;
            while (true)
            {
                Console.WriteLine("Resetting connection");
                try
                {
                    executionStart = DateTime.Now;
                    ResetPending = false;
                    ConsumeStream();
                    while (_isConsuming)
                        Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Output.Print(Name, "Exception from Run:" + Environment.NewLine + e);
                    while (_worker.IsBusy)
                    {
                        Console.WriteLine("Waiting for worker to finish");
                        Thread.Sleep(1000);
                    }
                    if ((DateTime.Now - executionStart).TotalSeconds < 5)
                    {
                        Output.Print(Name, "Previous failure was quick. Waiting 60 seconds.");
                        Thread.Sleep(59000);
                    }
                }
                Console.WriteLine(".");
                Thread.Sleep(1000);
            }
        }

        void CreateContext()
        {
            SingleUserAuthorizer authorizer = new SingleUserAuthorizer()
            {
                Credentials = GetCredentials()
            };
            _twitterContext = new TwitterContext(authorizer)
            {
                Log = Console.Out
            };
        }

        void ConsumeStream()
        {
            _isConsuming = true;
            int nonMessageCount = 0;

            if (_twitterContext == null)
                CreateContext();

            Console.WriteLine("Consuming stream");
            var selection = GetStreamQuery(_twitterContext);
            selection.StreamingCallback(stream =>
            {
                try
                {
                    //Did any connection error occur?
                    if (ResetPending
                        || stream.Status == TwitterErrorStatus.RequestProcessingException
                        || stream.Content == null
                        || nonMessageCount > 10)
                    {
                        stream.CloseStream();
                        _isConsuming = false;
                        return;
                    }

                    //Unhandled data
                    if (stream.Status != TwitterErrorStatus.Success)
                    {
                        Console.WriteLine(stream.Content);
                        nonMessageCount++;
                        return;
                    }

                    //Normal message
                    if (nonMessageCount > 0)
                        nonMessageCount--;

                    //if (stream.Content.StartsWith("{") && stream.Content.EndsWith("}"))
                    //    Console.Write(".");
                    //else
                    //    Console.Write(stream.Content.Substring(stream.Content.Length - Math.Min(stream.Content.Length, 3)));

                    _tweetQueue.Enqueue(stream.Content);
                    AllowAsyncDBWrite();
                }
                catch (Exception e)
                {
                    Output.Print(Name, e);
                    stream.CloseStream();
                    _isConsuming = false;
                    return;
                }
            })
            .SingleOrDefault();
        }

        object _workerLock = new object();
        void AllowAsyncDBWrite()
        {
            lock (_workerLock)
            {
                if (_worker.IsBusy || _tweetQueue.Count == 0 || (_tweetQueue.Count < 100 && (DateTime.Now - _lastDBWrite).TotalSeconds < 20))
                    return;

                _worker.RunWorkerAsync();
            }
        }

        void DBWrite(object sender, DoWorkEventArgs args)
        {
            //Check rate limit
            DateTime writeTime = DateTime.Now;
            int maxWrites = (int)Math.Ceiling(RateLimitPerMinute * (writeTime - _lastDBWrite).TotalMinutes);
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

            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(_connectionString);
                connection.Open();

                _dbFail = false;

                using (MySqlCommand command = connection.CreateCommand())
                {
                    SetInsertStatement(command, tweets);
                    try
                    {
                        Helpers.RunSqlStatement(Name, command);
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
                        Output.Print(Name, "Failed to connect to database:" + Environment.NewLine + e);
                        _dbFail = true;
                    }
                    else
                    {
                        Output.Print(Name, "Exception when inserting tweets:" + Environment.NewLine + e);
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
                    Output.Print(Name, "Exception when trying to close DB connection:" + Environment.NewLine + e);
                }
            }
        }

        void SetInsertStatement(MySqlCommand command, List<string> tweets)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO TweetJson (WordStatsOnly,Json) VALUES ");

            for (int i = 0; i < tweets.Count; i++)
            {
                string paramName = "@t" + i;
                if (i > 0)
                    sb.Append(",");
                sb.Append("(");
                sb.Append(_wordStatsOnly ? "1," : "0,");
                sb.Append(paramName);
                sb.Append(')');
                command.Parameters.AddWithValue(paramName, tweets[i]);
            }
            sb.Append(";");

            command.CommandText = sb.ToString();
        }

        void PrintJsonToFile(List<string> tweets)
        {
            if (string.IsNullOrEmpty(OutputFileDirectory))
                return;

            DateTime fileDate = DateTime.UtcNow.Date;

            try
            {
                using (TextWriter file = new StreamWriter(
                    path: OutputFileDirectory + "twitter_json_" + fileDate.ToString("yyyy-MM-dd") + ".txt",
                    append: true))
                {
                    foreach (string tweet in tweets)
                    {
                        file.WriteLine(tweet);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
