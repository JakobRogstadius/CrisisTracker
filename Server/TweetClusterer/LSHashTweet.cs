using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrisisTracker.Common;

namespace CrisisTracker.TweetClusterer
{
    class LSHashTweet
    {
        public long ID { get; set; }
        public long? RetweetOf { get; set; }
        public WordVector Vector { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is LSHashTweet)
                return ID.Equals(((LSHashTweet)obj).ID);
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
