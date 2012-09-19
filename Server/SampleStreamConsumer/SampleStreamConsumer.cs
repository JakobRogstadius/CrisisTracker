/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System.Net;
using CrisisTracker.Common;

namespace CrisisTracker.SampleStreamConsumer
{
    class SampleStreamConsumer : TwitterStreamConsumer
    {
        public SampleStreamConsumer()
            : base(Settings.ConnectionString, true)
        {
            Name = "SampleStreamConsumer";
        }


        protected override HttpWebRequest GetRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://stream.twitter.com/1/statuses/sample.json");
            webRequest.Method = "POST";
            webRequest.Credentials = new NetworkCredential(Settings.SampleStreamConsumer_Username, Settings.SampleStreamConsumer_Password);
            webRequest.ContentType = "application/x-www-form-urlencoded";

            return webRequest;
        }
    }
}
