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
        static void Main(string[] args)
        {
            //HelloLinqToTwitter();

            int rateLimitPerMinute;
            if (args.Length != 1 || !int.TryParse(args[0], out rateLimitPerMinute))
                rateLimitPerMinute = 150;
            if (rateLimitPerMinute < 1)
                rateLimitPerMinute = int.MaxValue;

            Console.WriteLine("SampleStreamConsumer");

            SampleStreamConsumer consumer = new SampleStreamConsumer(rateLimitPerMinute);
            consumer.OutputFileDirectory = "samplestream/";
            
            consumer.Run();
        }

        //static bool isRunning = false;
        //public static void HelloLinqToTwitter()
        //{
        //    while (true)
        //    {
        //        ConsumeStream();
        //        while (isRunning)
        //            System.Threading.Thread.Sleep(5000);
        //    }
        //}

        //static TwitterContext twitterContext = null;
        //static void CreateContext()
        //{
        //    // oauth application keys
        //    var consumerKey = "XD6Ygdgkja6yVfH9cTJ1pw";
        //    var consumerSecret = "LXYpx0FeAiAaRDKdRW2dU1YNCrqLGunVxWrfe5DQ0";
        //    var accessToken = "179047612-YVRB1ZxoXhd0y4hxtFflQTAe08VtbuHXfHo1u4lQ";
        //    var accessTokenSecret = "p4WAJdi40HUBZjgZuU6EeuyMm8wiPMsGQ4E1UJhzjYE";

        //    InMemoryCredentials credentials = new InMemoryCredentials()
        //    {
        //        ConsumerKey = consumerKey,
        //        ConsumerSecret = consumerSecret,
        //        AccessToken = accessTokenSecret,
        //        OAuthToken = accessToken
        //    };
        //    SingleUserAuthorizer authorizer = new SingleUserAuthorizer()
        //    {
        //        Credentials = credentials
        //    };
        //    twitterContext = new TwitterContext(authorizer)
        //    {
        //        Log = Console.Out
        //    };
        //}

        //static IQueryable<Streaming> GetStreamQuery()
        //{
        //    var selection = from strm in twitterContext.Streaming
        //                    where strm.Type == StreamingType.Filter &&
        //                        strm.Track == "twitter"
        //                    select strm;
        //    return selection;
        //}

        //static void ConsumeStream()
        //{
        //    isRunning = true;

        //    if (twitterContext == null)
        //        CreateContext();

        //    Console.WriteLine("Consuming Stream");

        //    int count = 0;
        //    var selection = GetStreamQuery();
        //    selection.StreamingCallback(stream =>
        //    {
        //        if (stream.Content.StartsWith("{") && stream.Content.EndsWith("}"))
        //            Console.WriteLine("ok");
        //        else
        //            Console.WriteLine("[[[ " + stream.Content + " ]]]");

        //        if (++count > 10)
        //        {
        //            stream.CloseStream();
        //            isRunning = false;
        //        }
        //    })
        //    .SingleOrDefault();
        //}







        //static void OAuthTest()
        //{
        //    //Oauth Keys (Replace with values that are obtained from registering 
        //    //the application at https://dev.twitter.com/apps/new
        //    string oAuthConsumerKey = "XD6Ygdgkja6yVfH9cTJ1pw";
        //    string oAuthConsumerSecret = "LXYpx0FeAiAaRDKdRW2dU1YNCrqLGunVxWrfe5DQ0";

        //    //Use the keys to get a bearer token
        //    string requestUrl = "https://api.twitter.com/oauth2/token";
        //    string authHeader = string.Format("Basic {0}", 
        //        Convert.ToBase64String(
        //            Encoding.UTF8.GetBytes(
        //                Uri.EscapeDataString(oAuthConsumerKey) + ":" +
        //                Uri.EscapeDataString(oAuthConsumerSecret))
        //        )
        //    );
        //    string requestBody = "grant_type=client_credentials";
        //    string method = "POST";

        //    HttpWebRequest request = CreateRequest(method, requestUrl, authHeader, requestBody);

        //    WebResponse response = request.GetResponse();   //Always Getting 500 Response Error
        //    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        //    Console.WriteLine(responseString);

        //    string bearerToken = ((Hashtable)JSON.JsonDecode(responseString))["access_token"].ToString();

        //    requestUrl = "https://stream.twitter.com/1.1/statuses/sample.json";
        //    authHeader = string.Format("Bearer {0}", bearerToken);

        //    request = CreateRequest("GET", requestUrl, authHeader, null);
        //    ConsumeRequest(request);
        //}

        //private static HttpWebRequest CreateRequest(string method, string requestUrl, string authHeader, string requestBody)
        //{
        //    ServicePointManager.Expect100Continue = false;

        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
        //    request.Headers.Add("Authorization", authHeader);
        //    request.Method = method;
        //    request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
        //    request.ProtocolVersion = new Version("1.1");

        //    if (requestBody != null && method != "GET") {
        //        using (Stream stream = request.GetRequestStream())
        //        {
        //            byte[] content = Encoding.UTF8.GetBytes(requestBody);
        //            stream.Write(content, 0, content.Length);
        //        }
        //    }

        //    return request;
        //}

        //static void ConsumeRequest(HttpWebRequest webRequest)
        //{
        //    using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
        //    {
        //        using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
        //        {
        //            for (int i = 0; i < 10; i++)
        //            {
        //                string streamContent = streamReader.ReadLine();
        //                Console.WriteLine(streamContent);
        //            }
        //        }
        //    }
        //}
    }
}