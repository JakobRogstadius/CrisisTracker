/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using CrisisTracker.Common;

namespace CrisisTracker.TweetClusterer
{
    class Program
    {
        const int _rehashIntervalHours = 1;
        const string Name = "CrisisTracker.TweetClusterer";

        static void Main(string[] args)
        {
            //test();
            //return;

            Console.WriteLine(Name);

            TweetClusterWorker clusterWorker = new TweetClusterWorker();
            
            clusterWorker.CreateHashTables();
            Console.WriteLine("Created hash tables");
            clusterWorker.InitializeWithOldTweets();

            DateTime nextRehash = DateTime.Now.AddHours(_rehashIntervalHours);
            int batchesBeforeStoryProcessing = 5;
            while (true)
            {
                DateTime start = DateTime.Now;

                //Calculate tweet relations and TweetClusterIDs
                int clusterBatchCount = 0;
                int processedCount = 0;
                do
                {
                    processedCount = clusterWorker.ProcessTweetBatch();
                    Console.WriteLine("Calculated relations");
                } while (processedCount > 10 && clusterBatchCount++ < batchesBeforeStoryProcessing);

                StoryWorker.ApplyPendingStorySplits();
                StoryWorker.ApplyPendingStoryMerges();
                StoryWorker.Run();

                //Update hash tables
                if (DateTime.Now > nextRehash)
                {
                    Console.WriteLine("Cleaning deleted tweets from hashtables");
                    clusterWorker.CleanDeletedOrArchivedTweets();
                    Console.WriteLine("Rehashing");
                    clusterWorker.UpdateOldestHashFunction();
                    nextRehash = nextRehash.AddHours(_rehashIntervalHours);
                    Console.WriteLine();
                }

                //Wait for up to 30 seconds
                int runtime = (int)(DateTime.Now - start).TotalMilliseconds;
                if (clusterBatchCount < batchesBeforeStoryProcessing && runtime < 30000)
                {
                    Console.WriteLine("Waiting");
                    Thread.Sleep(30000 - runtime);
                }

            }
        }

        static Random _rand = new Random();
        static int _dictionarySize = 50000;
        static void test()
        {
            string messageStr =
            @"{
                ""text"": ""Hi @bob, this message pretends to contain a tweet"", 
                ""aidr_labels"": [
                    { 
                        ""type"": ""nominallabel"",
                        ""attribute_id"": 3, 
                        ""attribute_name"": ""The color attribute"" 
                        ""label_code"": ""green"", 
                        ""label_name"": ""Green color"", 
                        ""source_id"": 1, 
                        ""confidence"": 0.6, 
                        ""from_human"": false 
                    },
                    { 
                        ""type"": ""nominallabel"",
                        ""attribute_id"": 4, 
                        ""attribute_name"": ""The speed attribute"" 
                        ""label_code"": ""fast"", 
                        ""label_name"": ""Going fast"", 
                        ""source_id"": 1, 
                        ""confidence"": 0.9, 
                        ""from_human"": false 
                    }
                ]
            }";
            var messageObj = Procurios.Public.JSON.JsonDecode(messageStr);
            String after = Procurios.Public.JSON.JsonEncode(((System.Collections.Hashtable)messageObj)["aidr_labels"]);
            var afterObj = Procurios.Public.JSON.JsonDecode(after);
            



            int planeCount = Settings.TweetClusterer_TCW_HyperPlaneCount;
            int tableCount = Settings.TweetClusterer_TCW_HashTableCount;
            int planeDimensions = Settings.TweetClusterer_TCW_WordsPerHyperPlane;
            int vectorLength = 6;
            int binSize = Settings.TweetClusterer_TCW_HashBinSize;

            //Create tables
            //List<int> allDimensions = GetRandomNumbers(_dictionarySize, tableCount * planeCount * planeDimensions);
            List<LSHashTable> tables = new List<LSHashTable>();
            int x = 0;
            for (int i = 0; i < tableCount; i++)
			{
                List<WordVector> planes = new List<WordVector>();
                for (int j = 0; j < planeCount; j++)
			    {
                    WordVector plane = new WordVector();
                    for (int k = 0; k < planeDimensions; k++)
                    {
                        //plane.AddItem(allDimensions[x++], Helpers.NextGaussian());
                        plane.AddItem(GetRandomNumber(_dictionarySize), Helpers.NextGaussian());
                    }
	                planes.Add(plane);
			    }
                LSHashFunction hash = new LSHashFunction(planes);
                tables.Add(new LSHashTable(hash, binSize));
			}

            LSHashTweetHistory history = new LSHashTweetHistory(500);

            //Make a needle
            LSHashTweet needle = GetNeedle(0, vectorLength);

            int needleCount = 0;
            int foundNeedleCount = 0;
            int anyNeighbors = 0;
            int simNeighbors = 0;
            int anyTrueCount = 0;

            for (int i = 1; i < 10000; i++)
            {
                bool isNeedle = _rand.NextDouble() < 0.02;
                LSHashTweet t;
                if (isNeedle)
                {
                    needleCount++;
                    t = GetNeedle(i, vectorLength);
                }
                else
                {
                    t = GetRandomTweet(i, vectorLength);
                }

                bool anyTrue = false;
                List<LSHashTweet> allHits = new List<LSHashTweet>();
                foreach (var table in tables)
                {
                    bool dummy;
                     allHits.AddRange(table.Add(t, out dummy));
                     anyTrue |= dummy;
                }
                var candidates = allHits
                    .GroupBy(n => n)
                    .OrderByDescending(n => n.Count())
                    .Take(3 * tableCount)
                    .OrderByDescending(n => n.Key.Vector * t.Vector);
                if (anyTrue)
                    anyTrueCount++;

                LSHashTweet best = null;
                if (candidates.Any())
                {
                    best = candidates.First().Key;
                }
                else
                {
                    List<LSHashTweet> historyMatch = history.GetNearestNeighbors(t, 1);
                    if (historyMatch.Any())
                        best = historyMatch.First();
                }

                double bestSim = 0;
                if (best != null)
                {
                    anyNeighbors++;
                    bestSim = best.Vector * t.Vector;
                    if (bestSim > 0.5)
                        simNeighbors++;
                }

                if (bestSim < 0.95)
                    history.Add(t);

                if (isNeedle && best != null && bestSim >= 1)
                    foundNeedleCount++;
            }
        }

        static LSHashTweet GetRandomTweet(int id, int size)
        {
            List<int> values = GetRandomNumbers(_dictionarySize, size);
            WordVector v = new WordVector();
            foreach (int i in values)
                v.AddItem(i, 1);
            v.Normalize();
            return new LSHashTweet() { ID = id, Vector = v };
        }

        static LSHashTweet GetNeedle(int id, int size)
        {
            WordVector v = new WordVector();
            for (int i=0; i<size; i++)
                v.AddItem(i, 1);
            v.Normalize();
            return new LSHashTweet() { ID = id, Vector = v };
        }

        static List<int> GetRandomNumbers(int maxValue, int count)
        {
            HashSet<int> values = new HashSet<int>();
            while (values.Count < count)
                values.Add(GetRandomNumber(maxValue));
            return values.ToList();
        }

        static int GetRandomNumber(int maxValue)
        {
            int rootMax = (int)Math.Pow(maxValue, 0.5);
            return _rand.Next(rootMax) * _rand.Next(rootMax);
        }
    }
}
