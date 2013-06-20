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

        public CustomBitArray CalculateHashScore(WordVector vector, out bool anyTrue)
        {
            anyTrue = false;
            CustomBitArray arr = new CustomBitArray(_hyperPlanes.Count);
            for (int i = _hyperPlanes.Count - 1; i != -1; i--)
            {
                anyTrue |= (arr[i] = (_hyperPlanes[i] * vector) > 0);
            }
            return arr;
        }
    }
}
