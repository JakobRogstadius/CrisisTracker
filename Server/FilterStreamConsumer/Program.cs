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
