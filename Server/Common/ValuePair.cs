﻿/*******************************************************************************
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

namespace CrisisTracker.Common
{
    public class ValuePair<T1, T2>
    {
        public T1 Value1;
        public T2 Value2;

        public override bool  Equals(object obj)
        {
            if (!(obj is ValuePair<T1,T2>))
                return false;
 	        ValuePair<T1,T2> other = (ValuePair<T1,T2>)obj;
            return Value1.Equals(other.Value1) && Value2.Equals(other.Value2);
        }

        public override int GetHashCode()
        {
            return Value1.GetHashCode();
        }

        public ValuePair()
        {

        }

        public ValuePair(T1 value1, T2 value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
    }
}
