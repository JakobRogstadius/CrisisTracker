/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Linq;
using LinqToTwitter;

namespace CrisisTracker.SampleStreamConsumer
{
    class Program
    {
        static int _defaultRateLimit = 150;

        static void Main(string[] args)
        {
            Console.WriteLine("SampleStreamConsumer");

            int rateLimitPerMinute;
            if (args.Length != 1 || !int.TryParse(args[0], out rateLimitPerMinute))
                rateLimitPerMinute = _defaultRateLimit;
            if (rateLimitPerMinute < 1)
                rateLimitPerMinute = int.MaxValue;

            Console.WriteLine("Database writes are limited to " + rateLimitPerMinute + " messages per minute.");
            if (rateLimitPerMinute == _defaultRateLimit)
                Console.WriteLine("Start with command line parameter [n] to change the rate limit (e.g. SampleStreamConsumer.exe 10000). Use a high value when populating an empty database with word statistics.");

            SampleStreamConsumer consumer = new SampleStreamConsumer(rateLimitPerMinute);
            consumer.OutputFileDirectory = "samplestream/";
            
            consumer.Run();
        }
    }
}