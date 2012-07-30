using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrisisTracker.TweetClusterer
{
    public class WeightedItem<T>
    {
        public T ItemID { get; set; }
        public double Score { get; set; }
    }
}
