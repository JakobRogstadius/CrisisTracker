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
