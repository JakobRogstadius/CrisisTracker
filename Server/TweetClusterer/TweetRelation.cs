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
    class TweetRelation
    {
        public Tweet N1 { get; private set; }
        public Tweet N2 { get; private set; }
        public bool IsBridge
        {
            get
            {
                //if (N1.StoryID.HasValue && N2.StoryID.HasValue)
                return N1.TweetClusterID != N2.TweetClusterID;
                //return false;
            }
        }
        public bool Deleted { get; set; }
        public double Similarity { get; set; }

        public TweetRelation(Tweet n1, Tweet n2)
        {
            if (n1 == n2)
                throw new Exception("Cannot create edge where both tweets are the same.");

            N1 = n1;
            N2 = n2;
            n1.Edges.Add(this);
            n2.Edges.Add(this);
        }

        public double GetNeighborOverlap()
        {
            HashSet<Tweet> neighbors1 = new HashSet<Tweet>();
            HashSet<Tweet> neighbors2 = new HashSet<Tweet>();

            neighbors1.Add(N1);
            foreach (TweetRelation edge1 in N1.RemainingEdges)
            {
                if (edge1 == this)
                    continue;

                if (edge1.N1 != N1)
                {
                    neighbors1.Add(edge1.N1);
                    foreach (TweetRelation edge2 in edge1.N1.RemainingEdges)
                    {
                        if (edge2.N1 != edge1.N1)
                            neighbors1.Add(edge2.N1);
                        else
                            neighbors1.Add(edge2.N2);

                        if (neighbors1.Count > 20)
                            continue;
                    }
                }
                else
                {
                    neighbors1.Add(edge1.N2);
                    foreach (TweetRelation edge2 in edge1.N2.RemainingEdges)
                    {
                        if (edge2.N1 != edge1.N2)
                            neighbors1.Add(edge2.N1);
                        else
                            neighbors1.Add(edge2.N2);

                        if (neighbors1.Count > 20)
                            continue;
                    }
                }
            }

            if (neighbors1.Count < 3)
                return 1;

            neighbors2.Add(N2);
            foreach (TweetRelation edge1 in N2.RemainingEdges)
            {
                if (edge1 == this)
                    continue;

                if (edge1.N1 != N2)
                {
                    neighbors2.Add(edge1.N1);
                    foreach (TweetRelation edge2 in edge1.N1.RemainingEdges)
                    {
                        if (edge2.N1 != edge1.N1)
                            neighbors2.Add(edge2.N1);
                        else
                            neighbors2.Add(edge2.N2);

                        if (neighbors2.Count > 20)
                            continue;
                    }
                }
                else
                {
                    neighbors2.Add(edge1.N2);
                    foreach (TweetRelation edge2 in edge1.N2.RemainingEdges)
                    {
                        if (edge2.N1 != edge1.N2)
                            neighbors2.Add(edge2.N1);
                        else
                            neighbors2.Add(edge2.N2);

                        if (neighbors2.Count > 20)
                            continue;
                    }
                }
            }

            if (neighbors2.Count < 3)
                return 1;

            int intersect = 0, union = 0;
            foreach (Tweet item in neighbors1)
            {
                if (neighbors2.Contains(item))
                    intersect++;
            }
            union = neighbors1.Count + neighbors2.Count - intersect;

            double score = (2 + intersect) / (double)union;
            return score;
        }
    }
}
