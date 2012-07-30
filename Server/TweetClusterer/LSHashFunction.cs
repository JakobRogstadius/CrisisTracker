using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using CrisisTracker.Common;

namespace CrisisTracker.TweetClusterer
{
    class LSHashFunction
    {
        List<WordVector> _hyperPlanes = new List<WordVector>();

        public LSHashFunction(IEnumerable<WordVector> hyperPlanes)
        {
            _hyperPlanes.AddRange(hyperPlanes);
        }

        public CustomBitArray CalculateHashScore(WordVector vector)
        {
            CustomBitArray arr = new CustomBitArray(_hyperPlanes.Count);
            for (int i = _hyperPlanes.Count - 1; i != -1; i--)
                arr[i] = (_hyperPlanes[i] * vector) >= 0;
            return arr;
        }
    }
}
