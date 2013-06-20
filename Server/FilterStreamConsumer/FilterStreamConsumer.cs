/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Net;
using System.IO;
using CrisisTracker.Common;
using System.Linq;
using LinqToTwitter;
using System.Linq.Expressions;

namespace CrisisTracker.FilterStreamConsumer
{
    class FilterStreamConsumer : TwitterStreamConsumer
    {
        FilterUpdateTracker _filterTracker;

        public FilterStreamConsumer()
            : base(Common.Settings.ConnectionString, false)
        {
            System.Net.ServicePointManager.Expect100Continue = false;

            Name = "FilterStreamConsumer";
            _filterTracker = new FilterUpdateTracker();
            _filterTracker.FiltersChanged += delegate(object sender, EventArgs e) { ResetPending = true; };
            _filterTracker.Start();
        }

        protected override InMemoryCredentials GetCredentials()
        {
            return new InMemoryCredentials()
            {
                ConsumerKey = Common.Settings.FilterStreamConsumer_ConsumerKey,
                ConsumerSecret = Common.Settings.FilterStreamConsumer_ConsumerSecret,
                OAuthToken = Common.Settings.FilterStreamConsumer_AccessToken, //LinkToTwitter uses strange parameter naming here
                AccessToken = Common.Settings.FilterStreamConsumer_AccessTokenSecret
            };
        }

        protected override IQueryable<Streaming> GetStreamQuery(TwitterContext context)
        {
            if (!_filterTracker.HasFilters)
                throw new Exception("No filters have been defined. Cannot start " + Name);

            Console.WriteLine("Track={0}", _filterTracker.GetKeywordFilterString());
            Console.WriteLine("Follow={0}", _filterTracker.GetUserFilterString());
            Console.WriteLine("Locations={0}", _filterTracker.GetGeoFilterString());

            var query = from stream in context.Streaming
                        where stream.Type == StreamingType.Filter
                        select stream;

            if (_filterTracker.HasKeywordFilters)
                query = query.Where(stream => stream.Track == _filterTracker.GetKeywordFilterString());

            if (_filterTracker.HasUserFilters)
                query = query.Where(stream => stream.Follow == _filterTracker.GetUserFilterString());

            if (_filterTracker.HasGeoFilters)
                query = query.Where(stream => stream.Locations == _filterTracker.GetGeoFilterString());

            return query;
        }
    }
}
