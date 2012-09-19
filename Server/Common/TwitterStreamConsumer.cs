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

namespace CrisisTracker.Common
{
    public abstract class TwitterStreamConsumer
    {
        DateTime _lastDBWrite;
        Queue<string> _tweetQueue;
        BackgroundWorker _worker;
        string _connectionString;
        bool _wordStatsOnly;
        bool _dbFail = false;

        public string OutputFileDirectory { get; set; }

        public bool ResetPending { get; set; }

        public string Name { get; set; }

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
                Console.Write("-");
                try
                {
                    executionStart = DateTime.Now;
                    ResetPending = false;
                    ConsumeStream();
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

        void ConsumeStream()
        {
            HttpWebRequest webRequest = GetRequest();

            if (webRequest == null)
                Output.Print(Name, "webRequest is null");

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse == null)
                    Output.Print(Name, "webResponse is null");

                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    if (streamReader == null)
                        Output.Print(Name, "streamReader is null");

                    int nonMessages = 0;
                    while (true)
                    {
                        if (ResetPending)
                            break;

                        string streamContent = streamReader.ReadLine();
                        if (streamContent == null) //Connection lost
                        {
                            Output.Print(Name, "Content of stream was null. Resetting connection.");
                            break;
                        }
                        else if (streamContent == "" || !streamContent.StartsWith("{")) //Keep-alive (\r) or unhandled data
                        {
                            nonMessages++;
                            if (nonMessages > 10)
                            {
                                Output.Print(Name, "Many non-messages in a row. Resetting connection.");
                                break; //10 non-messages in a row probably means that something went wrong
                            }
                            continue;
                        }
                        if (nonMessages > 0)
                            nonMessages--;

                        _tweetQueue.Enqueue(streamContent);

                        AllowAsyncDBWrite();
                    }

                    try
                    {
                        webRequest.Abort(); //The HttpWebRequest cannot be used again after this, because its WebResponse will be disposed.
                        streamReader.Close();
                        webResponse.Close();
                    }
                    catch (Exception e)
                    {
                        Output.Print(Name, "Exception when cleaning up connection resources:" + Environment.NewLine + e.ToString());
                    }
                }
            }

            webRequest = null;
            GC.Collect();
        }

        protected abstract HttpWebRequest GetRequest();

        object _workerLock = new object();
        void AllowAsyncDBWrite()
        {
            lock (_workerLock)
            {
                if (_worker.IsBusy || _tweetQueue.Count == 0 || (_tweetQueue.Count < 50 && (DateTime.Now - _lastDBWrite).TotalSeconds < 20))
                    return;

                _worker.RunWorkerAsync();
            }
        }

        void DBWrite(object sender, DoWorkEventArgs args)
        {
            int discardedCount = 0;
            List<string> tweets = new List<string>();
            while (_tweetQueue.Count > 0)
            {
                string tweet = _tweetQueue.Dequeue();
                if (tweets.Count < 500)
                    tweets.Add(tweet);
                else
                    discardedCount++;
            }

            if (discardedCount > 0)
                Output.Print(Name, "Discarded " + discardedCount + " tweets from the stream.");
            
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

            //PrintJsonToFile(tweets);

            _lastDBWrite = DateTime.Now;
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
