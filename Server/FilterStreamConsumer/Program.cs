/*******************************************************************************
 * Copyright (c) 2013 Jakob Rogstadius.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;

namespace CrisisTracker.FilterStreamConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("FilterStreamConsumer");

            FilterStreamConsumer consumer = new FilterStreamConsumer();
            consumer.Run();
        }
    }
}
