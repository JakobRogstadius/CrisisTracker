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
