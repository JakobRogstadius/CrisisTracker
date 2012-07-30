using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrisisTracker.TweetClusterer
{
    public class WeightedItemVector<T>
    {
        protected Dictionary<T, double> _itemWeights = new Dictionary<T, double>();
        protected bool _normalized = false;

        public void AddItem(T itemID, double weight, bool sumWeights = false)
        {
            if (_normalized)
                throw new Exception("Vector has already been normalized.");
            if (sumWeights && _itemWeights.ContainsKey(itemID))
                _itemWeights[itemID] += weight;
            else
                _itemWeights[itemID] = weight;
        }

        public double DotProduct(WeightedItemVector<T> b)
        {
            return this * b;
        }

        public static double operator *(WeightedItemVector<T> a, WeightedItemVector<T> b)
        {
            Dictionary<T, double> shorter, longer;
            if (a._itemWeights.Count < b._itemWeights.Count)
            {
                shorter = a._itemWeights;
                longer = b._itemWeights;
            }
            else
            {
                shorter = b._itemWeights;
                longer = a._itemWeights;
            }

            double product = 0;
            foreach (var itemWeight in shorter)
            {
                double weight;
                if (longer.TryGetValue(itemWeight.Key, out weight))
                    product += itemWeight.Value * weight;
            }

            return product;
        }

        public double DotProduct(T[] items)
        {
            double product = 0;
            foreach (T item in items)
            {
                double weight;
                if (_itemWeights.TryGetValue(item, out weight))
                    product += weight;
            }

            return product;
        }

        public double DotProduct(T[] items, Func<T, double> getWeight)
        {
            double product = 0;
            foreach (T item in items)
            {
                double weight;
                if (_itemWeights.TryGetValue(item, out weight))
                    product += getWeight(item) * weight;
            }

            return product;
        }

        public double SharedWordRatio(WeightedItemVector<T> other)
        {
            var topThis = this._itemWeights.OrderByDescending(n => n.Value).Select(n => n.Key).Take(5);
            var topOther = other._itemWeights.OrderByDescending(n => n.Value).Select(n => n.Key).Take(5);
            return topThis.Intersect(topOther).Count() / (double)topThis.Union(topOther).Count();
        }

        public List<T> GetItemIDs()
        {
            return new List<T>(_itemWeights.Keys);
        }

        public int ItemCount { get; private set; }

        public double OriginalLength { get; private set; }

        public void Normalize()
        {
            ItemCount = _itemWeights.Where(n => n.Value > 0).Count();
            OriginalLength = Math.Sqrt(_itemWeights.Values.Sum(n => n * n));

            T[] keys = _itemWeights.Keys.ToArray();
            double invLength = 1 / OriginalLength;
            foreach (var key in keys)
                _itemWeights[key] *= invLength;
        }

        public Dictionary<T, double> GetItemWeights()
        {
            return new Dictionary<T, double>(_itemWeights);
        }

        public double this[T item]
        {
            get
            {
                if (!_itemWeights.ContainsKey(item))
                    throw new ArgumentException("Item does not exist in vector.");
                return _itemWeights[item];
            }
            set
            {
                if (!_itemWeights.ContainsKey(item))
                    throw new ArgumentException("Item does not exist in vector.");
                _itemWeights[item] = value;
            }
        }

        public void ApplyIdfScores(Dictionary<T, double> itemScores)
        {
            foreach (var itemKey in _itemWeights.Keys.ToList())
            {
                if (itemScores.ContainsKey(itemKey))
                    _itemWeights[itemKey] *= itemScores[itemKey];
            }
        }

        public override string ToString()
        {
            return "(" + string.Join(",", _itemWeights.Select(n => n.Key + ":" + n.Value).ToArray()) + ")";
        }

        public void LimitComponentCount(int componentCount)
        {
            if (_itemWeights.Count <= componentCount)
                return;

            var toDelete = _itemWeights
                .OrderByDescending(n => n.Value)
                .Take(_itemWeights.Count - componentCount)
                .Select(n => n.Key).ToList();

            foreach (var item in toDelete)
                _itemWeights.Remove(item);
        }

        public double GetEntropy()
        {
            double sum = _itemWeights.Values.Sum();
            return -_itemWeights.Values.Sum(n => (n / sum) * Math.Log(n / sum));
        }
    }
}
