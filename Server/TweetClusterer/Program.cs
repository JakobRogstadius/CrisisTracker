/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Threading;

namespace CrisisTracker.TweetClusterer
{
    class Program
    {
        const int _rehashIntervalHours = 1;
        const string Name = "CrisisTracker.TweetClusterer";

        static void Main(string[] args)
        {
            Console.WriteLine(Name);

            TweetClusterWorker neighborFinder = new TweetClusterWorker();
            
            neighborFinder.CreateHashTables();
            Console.WriteLine("Created hash tables");
            neighborFinder.InitializeWithOldTweets();

            DateTime nextRehash = DateTime.Now.AddHours(_rehashIntervalHours);
            while (true)
            {
                DateTime start = DateTime.Now;

                //Calculate tweet relations and TweetClusterIDs
                int clusterBatchCount = 0;
                int processedCount = 0;
                do
                {
                    processedCount = neighborFinder.ProcessTweetBatch();

                    //These two are run here as they affect the percieved responsiveness of the front-end
                    StoryWorker.ApplyPendingStorySplits();
                    StoryWorker.ApplyPendingStoryMerges();
                } while (processedCount > 10 && clusterBatchCount++ < 10);
                Console.WriteLine("Calculated relations");

                //Perform agglomerative grouping of clusters into stories
                StoryWorker.Run();

                //Update hash tables
                if (DateTime.Now > nextRehash)
                {
                    Console.WriteLine("Cleaning deleted tweets from hashtables");
                    neighborFinder.CleanDeletedTweets();
                    Console.WriteLine("Rehashing");
                    neighborFinder.UpdateOldestHashFunction();
                    nextRehash = nextRehash.AddHours(_rehashIntervalHours);
                    Console.WriteLine();
                }

                //Wait for up to 30 seconds
                int runtime = (int)(DateTime.Now - start).TotalMilliseconds;
                if (runtime < 30000)
                {
                    Console.WriteLine("Waiting");
                    Thread.Sleep(30000 - runtime);
                }

            }
        }
    }
}
