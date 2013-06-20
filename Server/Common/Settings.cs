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

            _sampleStreamConsumer_ConsumerKey = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/consumerKey").InnerText;
            _sampleStreamConsumer_ConsumerSecret = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/consumerSecret").InnerText;
            _sampleStreamConsumer_AccessToken = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/accessToken").InnerText;
            _sampleStreamConsumer_AccessTokenSecret = xmlDoc.SelectSingleNode("/settings/sampleStreamConsumer/accessTokenSecret").InnerText;

            _filterStreamConsumer_ConsumerKey = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/consumerKey").InnerText;
            _filterStreamConsumer_ConsumerSecret = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/consumerSecret").InnerText;
            _filterStreamConsumer_AccessToken = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/accessToken").InnerText;
            _filterStreamConsumer_AccessTokenSecret = xmlDoc.SelectSingleNode("/settings/filterStreamConsumer/accessTokenSecret").InnerText;

            _tweetParser_WordScore4dMaxToStopwordRatio = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/wordScore4dMaxToStopwordRatio").InnerText, CultureInfo.InvariantCulture);
            _tweetParser_MaxWordTweetTableLength = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/maxWordTweetTableLength").InnerText);
            _tweetParser_BatchSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/batchSize").InnerText);
            _tweetParser_MinTweetVectorLength = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/minTweetVectorLength").InnerText, CultureInfo.InvariantCulture);
            _tweetParser_MinTweetWordCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetParser/minTweetWordCount").InnerText);

            _tweetClusterer_TCW_InitializeSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/initializeSize").InnerText);
            _tweetClusterer_TCW_BatchSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/batchSize").InnerText);
            _tweetClusterer_TCW_HistorySize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/historySize").InnerText);
            _tweetClusterer_TCW_HashTableCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hashTableCount").InnerText);
            _tweetClusterer_TCW_HyperPlaneCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hyperPlaneCount").InnerText);
            _tweetClusterer_TCW_WordsPerHyperPlane = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/wordsPerHyperPlane").InnerText);
            _tweetClusterer_TCW_HashBinSize = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/hashBinSize").InnerText);
            _tweetClusterer_TCW_MaxLinksPerTweet = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/maxLinksPerTweet").InnerText);
            _tweetClusterer_TCW_MinTweetVectorLength = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetVectorLength").InnerText);
            _tweetClusterer_TCW_MinTweetWordCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetWordCount").InnerText);
            _tweetClusterer_TCW_MinTweetSimilarityForLink = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/minTweetSimilarityForLink").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_TCW_IdentityThreshold = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/tweetClusterWorker/identityThreshold").InnerText, CultureInfo.InvariantCulture);

            _tweetClusterer_SW_MergeThreshold = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeThreshold").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_MergeThresholdWithDrop = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeThresholdWithDrop").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_MergeDropScale = double.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/mergeDropScale").InnerText, CultureInfo.InvariantCulture);
            _tweetClusterer_SW_TopStoryCount = int.Parse(xmlDoc.SelectSingleNode("/settings/tweetClusterer/storyWorker/topStoryCount").InnerText);
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
        private string _sampleStreamConsumer_ConsumerKey;
        private string _sampleStreamConsumer_ConsumerSecret;
        private string _sampleStreamConsumer_AccessToken;
        private string _sampleStreamConsumer_AccessTokenSecret;
        public static string SampleStreamConsumer_ConsumerKey { get { return GetInstance()._sampleStreamConsumer_ConsumerKey; } }
        public static string SampleStreamConsumer_ConsumerSecret { get { return GetInstance()._sampleStreamConsumer_ConsumerSecret; } }
        public static string SampleStreamConsumer_AccessToken { get { return GetInstance()._sampleStreamConsumer_AccessToken; } }
        public static string SampleStreamConsumer_AccessTokenSecret { get { return GetInstance()._sampleStreamConsumer_AccessTokenSecret; } }

        //FilterStreamConsumer
        private string _filterStreamConsumer_ConsumerKey;
        private string _filterStreamConsumer_ConsumerSecret;
        private string _filterStreamConsumer_AccessToken;
        private string _filterStreamConsumer_AccessTokenSecret;
        public static string FilterStreamConsumer_ConsumerKey { get { return GetInstance()._filterStreamConsumer_ConsumerKey; } }
        public static string FilterStreamConsumer_ConsumerSecret { get { return GetInstance()._filterStreamConsumer_ConsumerSecret; } }
        public static string FilterStreamConsumer_AccessToken { get { return GetInstance()._filterStreamConsumer_AccessToken; } }
        public static string FilterStreamConsumer_AccessTokenSecret { get { return GetInstance()._filterStreamConsumer_AccessTokenSecret; } }

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
        int _tweetClusterer_TCW_HashTableCount;
        int _tweetClusterer_TCW_HyperPlaneCount;
        int _tweetClusterer_TCW_WordsPerHyperPlane;
        int _tweetClusterer_TCW_HashBinSize;
        int _tweetClusterer_TCW_MaxLinksPerTweet;
        int _tweetClusterer_TCW_MinTweetVectorLength;
        int _tweetClusterer_TCW_MinTweetWordCount;
        double _tweetClusterer_TCW_MinTweetSimilarityForLink;
        double _tweetClusterer_TCW_IdentityThreshold;
        public static int TweetClusterer_TCW_InitializeSize { get { return GetInstance()._tweetClusterer_TCW_InitializeSize; } }
        public static int TweetClusterer_TCW_BatchSize { get { return GetInstance()._tweetClusterer_TCW_BatchSize; } }
        public static int TweetClusterer_TCW_HistorySize { get { return GetInstance()._tweetClusterer_TCW_HistorySize; } }
        public static int TweetClusterer_TCW_HashTableCount { get { return GetInstance()._tweetClusterer_TCW_HashTableCount; } }
        public static int TweetClusterer_TCW_HyperPlaneCount { get { return GetInstance()._tweetClusterer_TCW_HyperPlaneCount; } }
        public static int TweetClusterer_TCW_WordsPerHyperPlane { get { return GetInstance()._tweetClusterer_TCW_WordsPerHyperPlane; } }
        public static int TweetClusterer_TCW_HashBinSize { get { return GetInstance()._tweetClusterer_TCW_HashBinSize; } }
        public static int TweetClusterer_TCW_MaxLinksPerTweet { get { return GetInstance()._tweetClusterer_TCW_MaxLinksPerTweet; } }
        public static int TweetClusterer_TCW_MinTweetVectorLength { get { return GetInstance()._tweetClusterer_TCW_MinTweetVectorLength; } }
        public static int TweetClusterer_TCW_MinTweetWordCount { get { return GetInstance()._tweetClusterer_TCW_MinTweetWordCount; } }
        public static double TweetClusterer_TCW_MinTweetSimilarityForLink { get { return GetInstance()._tweetClusterer_TCW_MinTweetSimilarityForLink; } }
        public static double TweetClusterer_TCW_IdentityThreshold { get { return GetInstance()._tweetClusterer_TCW_IdentityThreshold; } }

        //StoryWorker
        double _tweetClusterer_SW_MergeThreshold;
        double _tweetClusterer_SW_MergeThresholdWithDrop;
        double _tweetClusterer_SW_MergeDropScale;
        int _tweetClusterer_SW_TopStoryCount;
        int _tweetClusterer_SW_TweetClusterBatchSize;
        int _tweetClusterer_SW_CandidateStoryCount;
        int _tweetClusterer_SW_MaxWordsInStoryVector;
        public static double TweetClusterer_SW_MergeThreshold { get { return GetInstance()._tweetClusterer_SW_MergeThreshold; } }
        public static double TweetClusterer_SW_MergeThresholdWithDrop { get { return GetInstance()._tweetClusterer_SW_MergeThresholdWithDrop; } }
        public static double TweetClusterer_SW_MergeDropScale { get { return GetInstance()._tweetClusterer_SW_MergeDropScale; } }
        public static int TweetClusterer_SW_TopStoryCount { get { return GetInstance()._tweetClusterer_SW_TopStoryCount; } }
        public static int TweetClusterer_SW_TweetClusterBatchSize { get { return GetInstance()._tweetClusterer_SW_TweetClusterBatchSize; } }
        public static int TweetClusterer_SW_CandidateStoryCount { get { return GetInstance()._tweetClusterer_SW_CandidateStoryCount; } }
        public static int TweetClusterer_SW_MaxWordsInStoryVector { get { return GetInstance()._tweetClusterer_SW_MaxWordsInStoryVector; } }
    }
}
