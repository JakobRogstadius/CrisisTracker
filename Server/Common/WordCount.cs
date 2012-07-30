using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EnglishStemmer;

namespace CrisisTracker.Common
{
    public class WordCount
    {
        Dictionary<string, uint> _wordCounts = new Dictionary<string, uint>();
        Dictionary<string, uint> _wordCountsTemp = new Dictionary<string, uint>();
        object _accessLock = new object();

        //MAINTAIN A CACHE OF RECENT WORD-STEMS

        public int MaxWordLength { get; set; }
        public int MinWordLength { get; set; }

        bool _locked = false;
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;

                lock (_accessLock)
                {
                    if (!_locked)
                    {
                        foreach (var wc in _wordCountsTemp)
                        {
                            if (_wordCounts.ContainsKey(wc.Key))
                                _wordCounts[wc.Key] += wc.Value;
                            else
                                _wordCounts.Add(wc.Key, wc.Value);
                        }
                        _wordCountsTemp.Clear();
                    }
                }
            }
        }

        public WordCount()
        {
            MaxWordLength = 30;
            MinWordLength = 2;
        }

        //static Regex nonWords = new Regex(
        //    @"http\S+|www\.\S+|[^("
        //    + @"\s|\w|[" //Latin
        //    + @"\u00C0-\u01BF" //Latin
        //    + @"\u01C4-\u02AB" //Latin
        //    + @"\u0386-\u0481" //Greek
        //    + @"\u048A-\u0513" //Cyrillic
        //    + @"\u0531-\u0556" //Armenian
        //    + @"\u0561-\u0587" //Armenian
        //    + @"\u05D0-\u05EA" //Hebrew
        //    + @"\u0621-\u063A" //Arabic
        //    + @"\u0641-\u065E" //Arabic
        //    + @"\u0660-\u0669" //Arabic
        //    + @"\u066E-\u06D3" //Arabic
        //    + @"\u06D0-\u06D9" //Arabic
        //    + @"\u070F-\u074A" //Syriac
        //    + @"\u074D-\u076D" //Syriac
        //    + "])]");

        public static string[] GetWordsInString(string text, bool useStemming = true)
        {
            //remove URLs and anything that is not a a-z, 0-9, whitespace //or _#@+-
            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"@\S+|http\S+|www\.\S+|[^(\s|\w|#)]|\(|\)", "");
            List<string> words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(n => n.Length > 2 && n.Length <= 30).ToList();

            if (!words.Any())
                return new string[0];
            else
            {
                foreach (string word in words.Where(n => n.StartsWith("#")).ToList())
                    words.Add(word.Substring(1)); //add the non-hashtag word

                if (!useStemming)
                    return words.ToArray();
                else
                {
                    return words.Select(w => NaiveStemming(w)).ToArray();
                    //return words.Select(w => WordStemCache.GetWordStem(w)).ToArray();
                }
            }
        }

        static Regex _stemmingPattern = new Regex("(es|ed|s|ing|ly|n)$");
        public static string NaiveStemming(string str)
        {
            if (str.Length < 4 || str.StartsWith("#"))
                return str;
            string before = str;
            while ((str = _stemmingPattern.Replace(str, "")) != before)
                before = str;
            return str;
        }

        public void AddWords(IEnumerable<string> words)
        {
            foreach (string word in words)
            {
                if (word.Length > MaxWordLength || word.Length < MinWordLength)
                    continue;

                lock (_accessLock)
                {
                    Dictionary<string, uint> wordCounts;
                    if (Locked)
                        wordCounts = _wordCountsTemp;
                    else
                        wordCounts = _wordCounts;

                    if (wordCounts.ContainsKey(word))
                        wordCounts[word]++;
                    else
                        wordCounts.Add(word, 1);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, uint>> GetWordCounts()
        {
            foreach (var wc in _wordCounts)
                yield return wc;
        }

        public IEnumerable<string> GetWords()
        {
            foreach (var wc in _wordCounts.Keys)
                yield return wc;
        }

        public void Clear()
        {
            _wordCounts.Clear();
        }

        public bool HasWords
        {
            get { return _wordCounts.Count > 0; }
        }

        public uint this[string word]
        {
            get
            {
                lock (_accessLock)
                {
                    if (!_wordCounts.ContainsKey(word))
                        return 0;
                    return _wordCounts[word];
                }
            }
        }
    }
}
