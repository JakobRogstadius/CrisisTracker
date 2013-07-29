/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
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
    class Tweet
    {
        public long ID;
        long _tweetClusterID;
        public long? OriginalTweetClusterID { get; private set; }
        public long TweetClusterID
        {
            get
            {
                return _tweetClusterID;
            }
            set
            {
                if (!OriginalTweetClusterID.HasValue)
                    OriginalTweetClusterID = value;
                _tweetClusterID = value;
            }
        }
        public bool TweetClusterIDChanged { get { return OriginalTweetClusterID.HasValue && OriginalTweetClusterID.Value != _tweetClusterID; } }
        public List<TweetRelation> Edges { get; protected set; }
        public IEnumerable<TweetRelation> RemainingEdges { get { return Edges.Where(e => !e.Deleted); } }

        public Tweet(long id)
        {
            Edges = new List<TweetRelation>();
            ID = id;
        }

        public bool IsOrHasNeighbor(Tweet n2)
        {
            if (n2 == this)
                return true;

            foreach (TweetRelation e in RemainingEdges)
            {
                if (e.N1 == n2 || e.N2 == n2)
                    return true;
            }
            return false;
        }

        public List<Tweet> GetNeighbors()
        {
            if (Edges == null || RemainingEdges.Count() == 0)
                return new List<Tweet>();

            return RemainingEdges.Select(n => n.N1 == this ? n.N2 : n.N1).ToList();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ID.Equals(obj);
        }
    }
}
