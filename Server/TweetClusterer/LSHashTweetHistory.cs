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

namespace CrisisTracker.TweetClusterer
{
    class LSHashTweetHistory
    {
        Queue<LSHashTweet> _content = new Queue<LSHashTweet>();
        Dictionary<long, List<LSHashTweet>> _wordIndex = new Dictionary<long, List<LSHashTweet>>();

        public int Capacity { get; private set; }

        public LSHashTweetHistory(int capacity)
        {
            Capacity = capacity;
        }

        public void Add(LSHashTweet item)
        {
            _content.Enqueue(item);
            List<long> wordIDs = item.Vector.GetItemIDs();
            foreach (long wordID in wordIDs)
            {
                if (!_wordIndex.ContainsKey(wordID))
                    _wordIndex.Add(wordID, new List<LSHashTweet>());
                _wordIndex[wordID].Add(item);
            }
            if (_content.Count > Capacity)
                RemoveOldest();
        }

        void RemoveOldest()
        {
            LSHashTweet tweet = _content.Dequeue();
            List<long> wordIDs = tweet.Vector.GetItemIDs();
            foreach (long wordID in wordIDs)
            {
                _wordIndex[wordID].Remove(tweet);
                if (_wordIndex[wordID].Count == 0)
                    _wordIndex.Remove(wordID);
            }
        }

        public List<LSHashTweet> GetNearestNeighbors(LSHashTweet tweet, int n)
        {
            if (_content.Count == 0)
                return new List<LSHashTweet>();

            HashSet<LSHashTweet> candidates = new HashSet<LSHashTweet>();
            List<long> wordIDs = tweet.Vector.GetItemIDs();
            foreach (long wordID in wordIDs)
            {
                if (_wordIndex.ContainsKey(wordID))
                {
                    foreach (LSHashTweet candidate in _wordIndex[wordID])
                        candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
                return new List<LSHashTweet>();

            return candidates.OrderByDescending(t => t.Vector * tweet.Vector).Take(n).ToList();
        }
    }
}
