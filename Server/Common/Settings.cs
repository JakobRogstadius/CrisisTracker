/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System.Xml;
using System.Globalization;

namespace CrisisTracker.Common
{
    public class Settings
    {
        private static Settings _instance = new Settings();

        private Settings()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("settings.xml");

            _connectionString = xmlDoc.SelectSingleNode("/settings/connectionString").InnerText;

            _sampleStreamConsumer_Username = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/twitterUsername").InnerText;
            _sampleStreamConsumer_Password = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/twitterPassword").InnerText;
            _sampleStreamConsumer_SourceID = int.Parse(xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/sourceID").InnerText);

            _filterStreamConsumer_Username = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/twitterUsername").InnerText;
            _filterStreamConsumer_Password = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/twitterPassword").InnerText;
            _filterStreamConsumer_SourceID = int.Parse(xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/sourceID").InnerText);

            _tweetParser_WordScore4dMaxToStopwordRatio = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/wordScore4dMaxToStopwordRatio").InnerText, CultureInfo.InvariantCulture);
            _tweetParser_MaxWordTweetTableLength = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/maxWordTweetTableLength").InnerText);
            _tweetParser_BatchSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/batchSize").InnerText);
            _tweetParser_MinTweetVectorLength = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/minTweetVectorLength").InnerText, CultureInfo.InvariantCulture);
            _tweetParser_MinTweetWordCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/minTweetWordCount").InnerText);

            _tweetClusterer_TCW_InitializeSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/initializeSize").InnerText);
            _tweetClusterer_TCW_BatchSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/batchSize").InnerText);
            _tweetClusterer_TCW_HistorySize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/historySize").InnerText);
            _tweetClusterer_TCW_DictionaryWordCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/dictionaryWordCount").InnerText);
            _tweetClusterer_TCW_HashTableCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hashTableCount").InnerText);
            _tweetClusterer_TCW_HyperPlaneCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hyperPlaneCount").InnerText);
            _tweetClusterer_TCW_HashBinSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hashBinSize").InnerText);
            _tweetClusterer_TCW_MaxLinksPerTweet = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/maxLinksPerTweet").InnerText);
            _tweetClusterer_TCW_MinTweetVectorLength = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetVectorLength").InnerText);
            _tweetClusterer_TCW_MinTweetWordCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetWordCount").InnerText);
            _tweetClusterer_TCW_MinTweetSimilarityForLink = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetSimilarityForLink").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_TCW_IdentityThreshold = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/identityThreshold").InnerText, CultureInfo.InvariantCulture);

            _tweetClusterer_SW_MergeUpperThreshold = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeUpperThreshold").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_MergeLowerThreshold = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeLowerThreshold").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_MergeDropScale = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeDropScale").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_TweetClusterBatchSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/tweetClusterBatchSize").InnerText);
            _tweetClusterer_SW_CandidateStoryCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/candidateStoryCount").InnerText);
            _tweetClusterer_SW_MaxWordsInStoryVector = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/maxWordsInStoryVector").InnerText);
        }

        private static Settings GetInstance()
        {
            if (_instance == null)
                _instance = new Settings();
            return _instance;
        }

        //Global
        private string _connectionString;
        public static string ConnectionString { get { return GetInstance()._connectionString; } }

        //SampleStreamConsumer
        private string _sampleStreamConsumer_Username;
        private string _sampleStreamConsumer_Password;
        private int _sampleStreamConsumer_SourceID;
        public static string SampleStreamConsumer_Username { get { return GetInstance()._sampleStreamConsumer_Username; } }
        public static string SampleStreamConsumer_Password { get { return GetInstance()._sampleStreamConsumer_Password; } }
        public static int SampleStreamConsumer_SourceID { get { return GetInstance()._sampleStreamConsumer_SourceID; } }

        //FilterStreamConsumer
        private string _filterStreamConsumer_Username;
        private string _filterStreamConsumer_Password;
        private int _filterStreamConsumer_SourceID;
        public static string FilterStreamConsumer_Username { get { return GetInstance()._filterStreamConsumer_Username; } }
        public static string FilterStreamConsumer_Password { get { return GetInstance()._filterStreamConsumer_Password; } }
        public static int FilterStreamConsumer_SourceID { get { return GetInstance()._filterStreamConsumer_SourceID; } }

        //TweetParser
        double _tweetParser_WordScore4dMaxToStopwordRatio;
        int _tweetParser_MaxWordTweetTableLength;
        int _tweetParser_BatchSize;
        double _tweetParser_MinTweetVectorLength;
        int _tweetParser_MinTweetWordCount;
        public static double TweetParser_WordScore4dMaxToStopwordRatio { get { return GetInstance()._tweetParser_WordScore4dMaxToStopwordRatio; } }
        public static int TweetParser_MaxWordTweetTableLength { get { return GetInstance()._tweetParser_MaxWordTweetTableLength; } }
        public static int TweetParser_BatchSize { get { return GetInstance()._tweetParser_BatchSize; } }
        public static double TweetParser_MinTweetVectorLength { get { return GetInstance()._tweetParser_MinTweetVectorLength; } }
        public static int TweetParser_MinTweetWordCount { get { return GetInstance()._tweetParser_MinTweetWordCount; } }

        //TweetClusterWorker
        int _tweetClusterer_TCW_InitializeSize;
        int _tweetClusterer_TCW_BatchSize;
        int _tweetClusterer_TCW_HistorySize;
        int _tweetClusterer_TCW_DictionaryWordCount;
        int _tweetClusterer_TCW_HashTableCount;
        int _tweetClusterer_TCW_HyperPlaneCount;
        int _tweetClusterer_TCW_HashBinSize;
        int _tweetClusterer_TCW_MaxLinksPerTweet;
        int _tweetClusterer_TCW_MinTweetVectorLength;
        int _tweetClusterer_TCW_MinTweetWordCount;
        double _tweetClusterer_TCW_MinTweetSimilarityForLink;
        double _tweetClusterer_TCW_IdentityThreshold;
        public static int TweetClusterer_TCW_InitializeSize { get { return GetInstance()._tweetClusterer_TCW_InitializeSize; } }
        public static int TweetClusterer_TCW_BatchSize { get { return GetInstance()._tweetClusterer_TCW_BatchSize; } }
        public static int TweetClusterer_TCW_HistorySize { get { return GetInstance()._tweetClusterer_TCW_HistorySize; } }
        public static int TweetClusterer_TCW_DictionaryWordCount { get { return GetInstance()._tweetClusterer_TCW_DictionaryWordCount; } }
        public static int TweetClusterer_TCW_HashTableCount { get { return GetInstance()._tweetClusterer_TCW_HashTableCount; } }
        public static int TweetClusterer_TCW_HyperPlaneCount { get { return GetInstance()._tweetClusterer_TCW_HyperPlaneCount; } }
        public static int TweetClusterer_TCW_HashBinSize { get { return GetInstance()._tweetClusterer_TCW_HashBinSize; } }
        public static int TweetClusterer_TCW_MaxLinksPerTweet { get { return GetInstance()._tweetClusterer_TCW_MaxLinksPerTweet; } }
        public static int TweetClusterer_TCW_MinTweetVectorLength { get { return GetInstance()._tweetClusterer_TCW_MinTweetVectorLength; } }
        public static int TweetClusterer_TCW_MinTweetWordCount { get { return GetInstance()._tweetClusterer_TCW_MinTweetWordCount; } }
        public static double TweetClusterer_TCW_MinTweetSimilarityForLink { get { return GetInstance()._tweetClusterer_TCW_MinTweetSimilarityForLink; } }
        public static double TweetClusterer_TCW_IdentityThreshold { get { return GetInstance()._tweetClusterer_TCW_IdentityThreshold; } }

        //StoryWorker
        double _tweetClusterer_SW_MergeUpperThreshold;
        double _tweetClusterer_SW_MergeLowerThreshold;
        double _tweetClusterer_SW_MergeDropScale;
        int _tweetClusterer_SW_TweetClusterBatchSize;
        int _tweetClusterer_SW_CandidateStoryCount;
        int _tweetClusterer_SW_MaxWordsInStoryVector;
        public static double TweetClusterer_SW_MergeUpperThreshold { get { return GetInstance()._tweetClusterer_SW_MergeUpperThreshold; } }
        public static double TweetClusterer_SW_MergeLowerThreshold { get { return GetInstance()._tweetClusterer_SW_MergeLowerThreshold; } }
        public static double TweetClusterer_SW_MergeDropScale { get { return GetInstance()._tweetClusterer_SW_MergeDropScale; } }
        public static int TweetClusterer_SW_TweetClusterBatchSize { get { return GetInstance()._tweetClusterer_SW_TweetClusterBatchSize; } }
        public static int TweetClusterer_SW_CandidateStoryCount { get { return GetInstance()._tweetClusterer_SW_CandidateStoryCount; } }
        public static int TweetClusterer_SW_MaxWordsInStoryVector { get { return GetInstance()._tweetClusterer_SW_MaxWordsInStoryVector; } }
    }
}
