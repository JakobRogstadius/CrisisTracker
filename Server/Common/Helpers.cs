using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Diagnostics;

namespace CrisisTracker.Common
{
    public static class Helpers
    {
        public enum BuildMode { Default, Baseline, WithStories, WithClusterRefinement }
        public const BuildMode CurrentBuildMode = BuildMode.Baseline;

        public static string EscapeSqlString(string value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\\'
                    || value[i] == '\''
                    || value[i] == '\"'
                    || value[i] == '`'
                    || value[i] == '´'
                    || value[i] == '’'
                    || value[i] == '‘')
                    sb.Append("\\");
                sb.Append(value[i]);
            }
            return sb.ToString();
        }

        //static readonly Encoding _iso = Encoding.GetEncoding("ISO-8859-1", new EncoderExceptionFallback(), new DecoderExceptionFallback());
        static readonly string _acceptedUrlChars = ":/.?=-~_@%#abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static List<string> GetUrlsInText(string text)
        {
            List<string> urls = new List<string>();

            //Extract segments containing :// or www.
            foreach (Match match in Regex.Matches(text, @"(www\.|[a-z]*://)[a-z0-9\./:\?=%-]*", RegexOptions.IgnoreCase))
            {
                if (match.Value != "")
                {
                    //Check if the match contains any non-permitted characters. This doesn't happen on Windows, but for some reason it does on Linux.
                    if (match.Value.ToCharArray().Any(n => !_acceptedUrlChars.Contains(n)))
                    {
                        Output.Print("Helpers.GetUrlsInText", "Url discarded: \"" + match.Value + "\"");
                    }
                    else
                    {
                        urls.Add(match.Value);
                    }
                }
            }

            return urls;
        }

        public static int RunSqlStatement(string callerName, string sql)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = sql;
            return RunSqlStatement(callerName, command); //Command gets disposed in RunSqlStatement
        }

        public static int RunSqlStatement(string callerName, MySqlCommand command)
        {
            if (command == null || command.CommandText == null || command.CommandText == "")
                return -1;

            int affectedRows = -1;

            if (!command.CommandText.Contains("start transaction"))
                command.CommandText = "start transaction; " + command.CommandText;
            if (!command.CommandText.Contains("commit;"))
                command.CommandText = command.CommandText + " commit;";

            MySqlConnection connection = null;
            try
            {
                bool keepTrying = false;
                int attemptCount = 0;
                do
                {
                    try
                    {
                        connection = new MySqlConnection(Settings.ConnectionString);
                        connection.Open();
                        command.Connection = connection;
                        command.CommandTimeout = 120;

                        affectedRows = command.ExecuteNonQuery();

                        command.Dispose();
                        connection.Close();
                        connection.Dispose();

                        keepTrying = false;
                    }
                    catch (MySqlException ex)
                    {
                        if (ex.Message.ToLowerInvariant().Contains("lock")) //Try again
                        {
                            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                            {
                                connection.Close();
                                connection.Dispose();
                            }

                            Console.Write("d");
                            keepTrying = true;
                            System.Threading.Thread.Sleep(2000 + _random.Next(2000 * attemptCount));
                            attemptCount++;
                        }
                        else
                            throw ex;
                    }

                    if (attemptCount == 20)
                        throw new Exception("20 deadlocks.");
                } while (keepTrying);
            }
            catch (Exception e)
            {
                string error = "Exception while running query:" + Environment.NewLine
                    + e + Environment.NewLine
                    + "Query was:" + Environment.NewLine
                    + command.CommandText;
                Output.Print(callerName, error);
            }
            finally
            {
                try
                {
                    if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
                catch (Exception e)
                {
                    Output.Print(callerName, "Exception while closing DB connection:" + Environment.NewLine + e);
                }
            }

            return affectedRows;
        }

        public delegate void Assigner<T>(T values, MySqlDataReader reader);

        public static void RunSelect<T>(string callerName, string sql, T values, Assigner<T> assigner)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = sql;
            RunSelect<T>(callerName, command, values, assigner); ;
        }

        public static void RunSelect<T>(string callerName, MySqlCommand command, T values, Assigner<T> assigner)
        {
            MySqlConnection connection = null;
            try
            {
                using (connection = new MySqlConnection(Settings.ConnectionString))
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandTimeout = 600;

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        HashSet<string> newKeywords = new HashSet<string>();
                        using (MySqlDataReader reader = adapter.SelectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                assigner(values, reader);
                            }
                            reader.Close();
                        }
                    }

                    command.Dispose();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Output.Print(callerName, "Exception while running query:" + Environment.NewLine
                    + e + Environment.NewLine
                    + "Query was:" + Environment.NewLine
                    + command.CommandText);
            }
            finally
            {
                try
                {
                    if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
                catch (Exception e)
                {
                    Output.Print(callerName, "Exception while closing DB connection:" + Environment.NewLine + e);
                }
            }
        }

        public static T RunSelectSingleValue<T>(string callerName, string sql)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = sql;
            T value = default(T);

            MySqlConnection connection = null;
            try
            {
                using (connection = new MySqlConnection(Settings.ConnectionString))
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandTimeout = 100;

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        HashSet<string> newKeywords = new HashSet<string>();
                        using (MySqlDataReader reader = adapter.SelectCommand.ExecuteReader())
                        {
                            value = (T)Convert.ChangeType(reader.Read(), typeof(T));
                        }
                    }

                    command.Dispose();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Output.Print(callerName, "Exception while running query:" + Environment.NewLine
                    + e + Environment.NewLine
                    + "Query was:" + Environment.NewLine
                    + command.CommandText);
            }
            finally
            {
                try
                {
                    if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
                catch (Exception e)
                {
                    Output.Print(callerName, "Exception while closing DB connection:" + Environment.NewLine + e);
                }
            }

            return value;
        }

        //public static double ScoreToIdf(double wordScore)
        //{
        //    return 8 / Math.Sqrt((wordScore + 20) * (1 + Math.Exp(-0.3 * (wordScore - 35))));
        //}

        static double? nextGaussian = null;
        static Random _random = new Random();
        public static double NextGaussian()
        {
            if (nextGaussian.HasValue)
            {
                double value = nextGaussian.Value;
                nextGaussian = null;
                return value;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * _random.NextDouble() - 1;   // between -1.0 and 1.0
                    v2 = 2 * _random.NextDouble() - 1;   // between -1.0 and 1.0
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
                nextGaussian = v2 * multiplier;
                return v1 * multiplier;
            }
        }

        /// <summary>
        /// Converts from pi to \u03a3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts from \u03a3 to pi
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    int character;
                    if (int.TryParse(m.Groups["Value"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out character))
                        return ((char)character).ToString();
                    else
                        return "";// m.Groups["Value"].Value;
                    //return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString(); 
                });
        }

    }
}
