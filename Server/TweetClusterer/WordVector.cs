using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrisisTracker.TweetClusterer
{
    public class WordVector : WeightedItemVector<long>
    {
        public WordVector Copy()
        {
            WordVector copy = new WordVector();
            foreach (var item in this._itemWeights)
                copy._itemWeights.Add(item.Key, item.Value);
            copy._normalized = this._normalized;

            return copy;
        }

        public static WordVector GetNormalizedAverage(IEnumerable<WordVector> vectors, int? maxComponentCount = null)
        {
            WordVector average = new WordVector();

            foreach (WordVector v in vectors)
            {
                foreach (var item in v._itemWeights)
                {
                    if (average._itemWeights.ContainsKey(item.Key))
                        average._itemWeights[item.Key] += item.Value * v.OriginalLength;
                    else
                        average._itemWeights.Add(item.Key, item.Value * v.OriginalLength);
                }
            }

            if (maxComponentCount.HasValue)
                average.LimitComponentCount(maxComponentCount.Value);
            
            average.Normalize();
            return average;
        }
    }
}
