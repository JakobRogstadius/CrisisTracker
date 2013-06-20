/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

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
        public List<LSHashTweet> Add(LSHashTweet tweet, out bool anyTrue)
        {
            lock (accessLock)
            {
                CustomBitArray hash = _hashFunction.CalculateHashScore(tweet.Vector, out anyTrue);
                LSHashTableCell cell;
                if (_values.TryGetValue(hash, out cell))
                {
                    List<LSHashTweet> neighbors = cell.GetTweets();
                    cell.Add(tweet);
                    _lastUpdatedCell = cell;
                    if (anyTrue)
                        return neighbors;
                    return new List<LSHashTweet>();
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
                        bool anyTrue = false;
                        CustomBitArray hash = _hashFunction.CalculateHashScore(tweet.Vector, out anyTrue);
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

        public IEnumerable<long> GetTweetIDs() { return _values.Values.SelectMany(n => n.GetTweetIDs()).Distinct(); }

        public void RemoveTweetsByID(IEnumerable<long> tweetIDs)
        {
            foreach (LSHashTableCell cell in _values.Values)
            {
                cell.RemoveTweetsByID(tweetIDs);
            }
        }
    }
}
