/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Threading;
using System.Linq;
using LinqToTwitter;
using CrisisTracker.Common;
using ServiceStack.Redis;

namespace CrisisTracker.FilterStreamConsumerAidr
{
    public abstract class TwitterStreamConsumerAidr
    {
        DateTime _lastDBWrite = new DateTime(1000, 1, 1);
        bool _isConsuming = false;
        TwitterContext _twitterContext = null;
        string _redisChannel;
        string _crisisCode;
        string _aidrJson;
        BasicRedisClientManager _redis;

        public string OutputFileDirectory { get; set; }

        public bool ResetPending { get; set; }

        public string Name { get; set; }

        protected abstract InMemoryCredentials GetCredentials();
        protected abstract IQueryable<Streaming> GetStreamQuery(TwitterContext context);

        public TwitterStreamConsumerAidr(string redisChannel, string crisisCode)
        {
            _redisChannel = redisChannel + "." + crisisCode;
            _crisisCode = crisisCode;
            _aidrJson = @",""aidr"":{{""crisis_code"":""" + _crisisCode + @".{0}"",""doctype"":""twitter""}}}}";
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
                    _redis = new BasicRedisClientManager(CrisisTracker.Common.Settings.AidrRedisConsumer_RedisHost + ":" + CrisisTracker.Common.Settings.AidrRedisConsumer_RedisPort);
                    ConsumeStream();
                    while (_isConsuming)
                        Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Output.Print(Name, "Exception from Run:" + Environment.NewLine + e);
                }
                finally
                {
                    Console.WriteLine("Disposing redis client");
                    _redis.Dispose();
                }
                if ((DateTime.Now - executionStart).TotalSeconds < 5)
                {
                    Output.Print(Name, "Previous failure was quick. Waiting 60 seconds.");
                    Thread.Sleep(59000);
                }
                Console.WriteLine(".");
                Thread.Sleep(1000);
            }
        }

        void CreateContext()
        {
            if (_twitterContext != null)
                _twitterContext.Dispose();

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

                    if (stream.Content.StartsWith("{") && stream.Content.EndsWith("}"))
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write("x");
                        return;
                    }

                    redisPublish(stream.Content);
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

        private void redisPublish(string jsonTweet)
        {
            string lang = GetLanguage(jsonTweet);
            if (lang == null)
            {
                Console.WriteLine("\n" + jsonTweet);
                return;
            }
            jsonTweet = InjectAidrField(jsonTweet, lang);
            using (var redis = _redis.GetClient())
            {
                redis.PublishMessage(_redisChannel + "." + lang, jsonTweet);
            }
        }

        private string InjectAidrField(string jsonTweet, string lang)
        {
            return jsonTweet.Substring(0, jsonTweet.Length - 1) + String.Format(_aidrJson, lang);
        }

        private string GetLanguage(string jsonTweet)
        {
            //This may get the lang field in a retweeted status, but it shouldn't matter
            int start = jsonTweet.IndexOf("\"lang\":") + 8;
            if (start < 9)
                return null;
            int end = jsonTweet.IndexOf('"', start);
            return jsonTweet.Substring(start, end - start);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
