/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System.Net;
using CrisisTracker.Common;
using LinqToTwitter;
using System.Linq;

namespace CrisisTracker.SampleStreamConsumer
{
    class SampleStreamConsumer : TwitterStreamConsumer
    {
        public SampleStreamConsumer(int rateLimit)
            : base(CrisisTracker.Common.Settings.ConnectionString, useForWordStatsOnly: true)
        {
            Name = "SampleStreamConsumer";
            
            RateLimitPerMinute = rateLimit;
        }

        protected override InMemoryCredentials GetCredentials()
        {
            return new InMemoryCredentials()
            {
                ConsumerKey = Common.Settings.SampleStreamConsumer_ConsumerKey,
                ConsumerSecret = Common.Settings.SampleStreamConsumer_ConsumerSecret,
                OAuthToken = Common.Settings.SampleStreamConsumer_AccessToken, //LinkToTwitter uses strange parameter naming here
                AccessToken = Common.Settings.SampleStreamConsumer_AccessTokenSecret
            };
        }

        protected override IQueryable<Streaming> GetStreamQuery(TwitterContext context)
        {
            var selection = from stream in context.Streaming
                            where stream.Type == StreamingType.Sample
                            select stream;
            return selection;
        }
    }
}
