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
using System.Globalization;

namespace CrisisTracker.Common
{
    public class TrackFilter
    {
        public enum FilterType { Undefined = -1, Word = 0, User = 1, Region = 2 }

        public int? ID { get; set; }
        public bool? IsStrong { get; set; }
        public FilterType Type { get; set; }
        private string[] _words;
        private string[] _stemmedWords;
        public void SetWords(string words)
        {
            _words = words.Split(' ');
            _stemmedWords = _words.Select(n => WordCount.NaiveStemming(n)).ToArray();
        }
        public long? UserID { get; set; }
        public double? Longitude1 { get; set; }
        public double? Longitude2 { get; set; }
        public double? Latitude1 { get; set; }
        public double? Latitude2 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is TrackFilter)
                return ID.Equals(((TrackFilter)obj).ID);
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            switch (Type)
            {
                case FilterType.Word:
                    return string.Join(" ", _words);
                case FilterType.User:
                    return UserID.ToString();
                case FilterType.Region:
                    return Longitude1 + "," + Latitude1 + "," + Longitude2 + "," + Latitude2;
                default:
                    return "";
            }
        }

        public bool Match(string[] words, long userID, double? longitude, double? latitude, bool useStemming=true)
        {
            switch (Type)
            {
                case FilterType.Word:
                    if (words == null || words.Length == 0 || _words == null || _words.Length == 0)
                        return false;
                    if (_words.Length == 1)
                    {
                        if (useStemming)
                            return words.Contains(_stemmedWords[0]);
                        else
                            return words.Contains(_words[0]);
                    }
                    else
                    {
                        if (useStemming)
                            return _stemmedWords.Count(n => words.Contains(n)) == _stemmedWords.Length;
                        else
                            return _words.Count(n => words.Contains(n)) == _words.Length;
                    }
                case FilterType.User:
                    if (userID == UserID)
                        return true;
                    return false;
                case FilterType.Region:
                    if (longitude.HasValue && latitude.HasValue
                        && longitude > Longitude1 && longitude < Longitude2
                        && latitude > Latitude1 && latitude < Latitude2)
                        return true;
                    return false;
                default:
                    return false;
            }
        }
    }
}
