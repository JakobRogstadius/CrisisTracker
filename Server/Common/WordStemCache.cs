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

namespace CrisisTracker.Common
{
    public class WordStemCache
    {
        static WordStemCache _instance;

        static WordStemCache GetInstance()
        {
            if (_instance == null)
                _instance = new WordStemCache();

            return _instance;
        }

        Dictionary<string, string> _wordStems = new Dictionary<string,string>();
        LinkedList<string> _wordQueue = new LinkedList<string>();

        public static string GetWordStem(string word)
        {
            return GetInstance().InternalGetWordStem(word);
        }

        string InternalGetWordStem(string word)
        {
            string stem;
            if (_wordStems.TryGetValue(word, out stem))
            {
                BringToFront(word);
                return stem;
            }
            else
            {
                stem = new EnglishStemmer.EnglishWord(word).Stem;
                InsertWord(word, stem);
                return stem;
            }
        }

        void BringToFront(string word)
        {
            _wordQueue.Remove(word);
            _wordQueue.AddFirst(word);
        }

        void InsertWord(string word, string stem)
        {
            if (_wordStems.Count > 1000)
            {
                string lastWord = _wordQueue.Last();
                _wordQueue.RemoveLast();
                _wordStems.Remove(lastWord);
            }

            _wordStems.Add(word, stem);
            _wordQueue.AddFirst(word);
        }
    }
}
