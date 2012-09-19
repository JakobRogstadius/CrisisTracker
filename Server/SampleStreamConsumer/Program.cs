/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace CrisisTracker.SampleStreamConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SampleStreamConsumer");

            SampleStreamConsumer consumer = new SampleStreamConsumer();
            consumer.OutputFileDirectory = "samplestream/";
            consumer.Run();
        }
    }
}
