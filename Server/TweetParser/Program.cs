using System;
using CrisisTracker.Common;

namespace CrisisTracker.TweetParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TweetParser");

            TweetParser parser = new TweetParser();
            parser.Run();
        }
    }
}
