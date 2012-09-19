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

        protected override HttpWebRequest GetRequest()
        {
            string trackString = _filterTracker.GetTrackString();
            Console.WriteLine(trackString);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://stream.twitter.com/1/statuses/filter.json");
            webRequest.Method = "POST";
            webRequest.Credentials = new NetworkCredential(Settings.FilterStreamConsumer_Username, Settings.FilterStreamConsumer_Password);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = trackString.Length;

            //Add track keywords as parameters to request
            using (StreamWriter stOut = new StreamWriter(webRequest.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                stOut.Write(trackString);
            }

            return webRequest;
        }
    }
}
