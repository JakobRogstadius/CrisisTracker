using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace CrisisTracker.Common
{
    public class Output
    {
        static object _writeLock = new object();

        public static void Print(string sourceName, Exception e)
        {
            Print(sourceName, e.ToString());
        }

        public static void Print(string sourceName, string text)
        {
            string path = "debug.txt";
            if (path == "console")
            {
                Console.WriteLine(Timestamp + text);
            }
            else if (path != "" && path != null)
            {
                WriteToFile(sourceName, text, path);
            }
        }

        public static void WriteToFile(string sourceName, string text, string path)
        {
            lock (_writeLock)
            {
                using (TextWriter tw = new StreamWriter(path, true))
                {
                    tw.WriteLine(Timestamp + sourceName + ": " + text);
                }
            }
        }

        public static void WriteToFile(string text, string path)
        {
            lock (_writeLock)
            {
                using (TextWriter tw = new StreamWriter(path, true))
                {
                    tw.WriteLine(text);
                }
            }
        }

        static string Timestamp
        {
            get { return "[" + DateTime.Now.ToString("d MMM HH:mm:ss") + "] "; }
        }
    }
}
