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
using CrisisTracker.Common;

namespace CrisisTracker.TweetClusterer
{
    class LSHashTweet
    {
        public long ID { get; set; }
        public long? RetweetOf { get; set; }
        public WordVector Vector { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is LSHashTweet)
                return ID.Equals(((LSHashTweet)obj).ID);
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
