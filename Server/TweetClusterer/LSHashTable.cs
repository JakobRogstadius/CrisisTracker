using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CrisisTracker.TweetClusterer
{
    class LSHashTable
    {
        int _cellCapacity;
        LSHashFunction _hashFunction;
        Dictionary<CustomBitArray, LSHashTableCell> _values = new Dictionary<CustomBitArray, LSHashTableCell>();
        object accessLock = new object();
        LSHashTableCell _lastUpdatedCell = null;

        public LSHashTable(LSHashFunction hashFunction, int cellCapacity)
        {
            _hashFunction = hashFunction;
            _cellCapacity = cellCapacity;
        }

        /// <summary>
        /// Adds the vector to the hashtable and returns any previously added tweets with the same hash value
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public List<LSHashTweet> Add(LSHashTweet tweet)
        {
            lock (accessLock)
            {
                CustomBitArray hash = _hashFunction.CalculateHashScore(tweet.Vector);
                LSHashTableCell cell;
                if (_values.TryGetValue(hash, out cell))
                {
                    List<LSHashTweet> neighbors = cell.GetTweets();
                    cell.Add(tweet);
                    _lastUpdatedCell = cell;
                    return neighbors;
                }
                else
                {
                    cell = new LSHashTableCell(_cellCapacity);
                    cell.Add(tweet);
                    _values.Add(hash, cell);
                    _lastUpdatedCell = cell;
                    return new List<LSHashTweet>();
                }
            }
        }

        public void UndoLastAdd()
        {
            if (_lastUpdatedCell != null)
                _lastUpdatedCell.UndoLastAdd();
            _lastUpdatedCell = null;
        }

        public void SetNewHashFunction(LSHashFunction hashFunction)
        {
            lock (accessLock)
            {
                Dictionary<CustomBitArray, LSHashTableCell> oldValues = _values;
                _values = new Dictionary<CustomBitArray, LSHashTableCell>();
                _hashFunction = hashFunction;

                //Re-hash all the old tweets using the new hash function
                foreach (LSHashTableCell oldCell in oldValues.Values)
                {
                    foreach (LSHashTweet tweet in oldCell.GetTweets())
                    {
                        CustomBitArray hash = _hashFunction.CalculateHashScore(tweet.Vector);
                        LSHashTableCell cell;
                        if (_values.TryGetValue(hash, out cell))
                        {
                            cell.Add(tweet);
                        }
                        else
                        {
                            cell = new LSHashTableCell(_cellCapacity);
                            cell.Add(tweet);
                            _values.Add(hash, cell);
                        }
                    }
                }
            }
        }

        public int GetItemCount()
        {
            return _values.Values.Sum(n => n.Count);
        }

        public HashSet<long> GetTweetIDs()
        {
            HashSet<long> tweetIDs = new HashSet<long>();
            foreach (LSHashTableCell cell in _values.Values)
            {
                var values = cell.GetTweets();
                foreach (var item in values)
                {
                    tweetIDs.Add(item.ID);
                }
            }
            return tweetIDs;
        }

        public void RemoveTweetsByID(IEnumerable<long> tweetIDs)
        {
            foreach (LSHashTableCell cell in _values.Values)
            {
                cell.RemoveTweetsByID(tweetIDs);
            }
        }
    }
}
