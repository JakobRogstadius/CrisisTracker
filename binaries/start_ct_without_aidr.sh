(cd ct && screen -U -dmS SampleConsumer mono SampleStreamConsumer.exe)
(cd ct && screen -U -dmS FilterConsumer mono FilterStreamConsumer.exe)
(cd ct && screen -U -dmS TweetParser mono TweetParser.exe)
(cd ct && screen -U -dmS TweetClusterer mono TweetClusterer.exe)
