<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

function get_story_content($storyID, $sortOrder, $db_conn, $includeRelatedStories = true) {
  $story = array('storyID' => $storyID);
  
  // Story info
  $storyInfoResult = mysql_query(
  "select 
      Title,
      CustomTitle,
      UserCount,
      TweetCount,
      RetweetCount,
      StartTime,
      EndTime,
      ShortDate(StartTime) as StartTimeShort,
      ShortDate(EndTime) as EndTimeShort,
      case when IsHidden then 1 else 0 end as IsHidden
    from Story left join StoryCustomTitle t on t.StoryID=Story.StoryID
    where Story.StoryID = $storyID", $db_conn);
  $storyInfo = mysql_fetch_array($storyInfoResult);
  
  $story['title'] = $storyInfo['Title'];
  $story['customTitle'] = urldecode($storyInfo['CustomTitle']);
  $story['userCount'] = $storyInfo['UserCount'];
  $story['tweetCount'] = $storyInfo['TweetCount'];
  $story['retweetCount'] = $storyInfo['RetweetCount'];
  $story['startTime'] = $storyInfo['StartTime'];
  $story['endTime'] = $storyInfo['EndTime'];
  $story['startTimeShort'] = $storyInfo['StartTimeShort'];
  $story['endTimeShort'] = $storyInfo['EndTimeShort'];
  $story['isHidden'] = $storyInfo['IsHidden'];
  
  /*
  // URLs
  $storyUrlsResult = mysql_query(
  "select Url
  from TweetUrl natural join Tweet natural join TweetCluster
  where StoryID = $storyID
  group by 1
  order by count(*) desc, CreatedAt
  limit 10", $db_conn);
  
  if (mysql_num_rows($storyUrlsResult) > 0) {
    $story['topUrls'] = array();
    while($row = mysql_fetch_array($storyUrlsResult)) {
      $story['topUrls'][] = htmlspecialchars($row['Url']);
    }
  }
  */
  
  // First Tweet
  $firstTweetResult = mysql_query(
  "select TweetID, RealName, ScreenName, CreatedAt, ShortDate(CreatedAt) as CreatedAtShort, Text
  from Tweet natural join TwitterUser natural join TweetCluster
  where StoryID=$storyID
  order by CreatedAt
  limit 1", $db_conn);
  $firstTweet = mysql_fetch_array($firstTweetResult);
  
  $story['firstTweet'] = array();
  $story['firstTweet']['tweetID'] = $firstTweet['TweetID'];
  $story['firstTweet']['screenName'] = htmlspecialchars($firstTweet['ScreenName']);
  $story['firstTweet']['realName'] = $firstTweet['RealName'];
  $story['firstTweet']['createdAt'] = $firstTweet['CreatedAt'];
  $story['firstTweet']['createdAtShort'] = $firstTweet['CreatedAtShort'];
  $story['firstTweet']['text'] = $firstTweet['Text'];
  
  
  $orderby = 'TweetCount desc';
  if ($sortOrder != 'size')
    $orderby = 'CreatedAt';
  
  if ($sortOrder == "first50") {
    // First 50 versions
    $storySummaryResult = mysql_query(
    "select * from (
      select T.TweetCount, t.TweetID, T.TweetClusterID, t.Text, t.CreatedAt, ShortDate(t.CreatedAt) as CreatedAtShort, u.ScreenName, u.RealName, u.UserID
      from (
          select Tweet.TweetClusterID, min(TweetID) as FirstID, count(*) as TweetCount from Tweet natural join TweetCluster where StoryID=$storyID
          group by lcase(IF(left(Text, 30) REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2, 30), left(Text, 30)))
      ) T
      join Tweet t on t.TweetID = T.FirstID
      join TwitterUser u on u.UserID = t.UserID
      where Text!=''
      order by t.CreatedAt
      limit 50) T2
    order by $orderby", $db_conn);    
  }
  else {
    // Summary of tweets
    $storySummaryResult = mysql_query(
    "select * from (
        select T.TweetCount, t.TweetID, T.TweetClusterID, t.Text, t.CreatedAt, ShortDate(t.CreatedAt) as CreatedAtShort, u.ScreenName, u.RealName, u.UserID
        from (
            select Tweet.TweetClusterID, min(TweetID) as FirstID, count(*) as TweetCount from Tweet natural join TweetCluster where StoryID=$storyID
            group by lcase(IF(left(Text, 30) REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2, 30), left(Text, 30)))
        ) T
        join Tweet t on t.TweetID = T.FirstID
        join TwitterUser u on u.UserID = t.UserID
        left join PendingStorySplits pss on pss.TweetClusterID = t.TweetClusterID
        where Text!='' and pss.TweetClusterID is null
        order by TweetCount desc
        limit 50) T2
    order by $orderby", $db_conn);
  }
  
  if (mysql_num_rows($storySummaryResult) > 0) {
    $story['topTweets'] = array();
    
    while($row = mysql_fetch_array($storySummaryResult)) {
      $tweet = array();
      $tweet['firstCreatedAt'] = $row['CreatedAt'];
      $tweet['firstCreatedAtShort'] = $row['CreatedAtShort'];
      $tweet['firstTweetID'] = $row['TweetID'];
      $tweet['firstUserID'] = $row['UserID'];
      $tweet['firstScreenName'] = htmlspecialchars($row['ScreenName']);
      $tweet['firstRealName'] = htmlspecialchars($row['RealName']);
      $tweet['tweetClusterID'] = $row['TweetClusterID'];
      $tweet['count'] = $row['TweetCount'];
      $tweet['text'] = $row['Text'];
  
      $story['topTweets'][] = $tweet;
    }
  }
  
  if ($includeRelatedStories) {
    // Related stories
/*    $relatedStoriesResult = mysql_query(
      "select 
          StoryID2 as StoryID,
          Title,
          ShortDate(StartTime) as StartTimeShort,
          ceil(least(UserCount, TweetCount + 0.5*log(10+RetweetCount))) as Popularity
      from (
          select
              StoryID1, 
              StoryID2, 
              (1+log10(UserCount)) * 0.5*T.CommonTags/(T.TagCount1+count(*)) / (1+0.2*dayDiff) as Similarity, 
              CommonTags
          from (
              select
                  s1.StoryID as StoryID1,
                  s2.StoryID as StoryID2,
                  abs(StartTime1 - unix_timestamp(s2.StartTime))/86400 as dayDiff,
                  s2.UserCount,
                  TagCount1,
                  count(*) as CommonTags
              from
                  (
                      select s.StoryID, unix_timestamp(s.StartTime) as StartTime1, count(*) as TagCount1
                      from Story s
                        left join StoryInfoKeywordTag t on t.StoryID=s.StoryID
                      where s.StoryID = " . $storyID . "
                      group by s.StoryID
                  ) s1
                  join StoryInfoKeywordTag t1 on t1.StoryID=s1.StoryID
                  join StoryInfoKeywordTag t2 on t2.InfoKeywordID=t1.InfoKeywordID and t2.StoryID!=t1.StoryID
                  join Story s2 on s2.StoryID=t2.StoryID
                  left join PendingStoryMerges psm on psm.StoryID2 = s2.StoryID
                  where psm.StoryID2 is null
              group by s1.StoryID, s2.StoryID
          ) T
          join StoryInfoKeywordTag t3 on t3.StoryID=T.StoryID2
          group by StoryID1, StoryID2
          order by StoryID1, Similarity desc, StoryID2
          limit 10
      ) T
      join Story on Story.StoryID=T.StoryID2
      ;", $db_conn);
*/


    /* Query looks complicated, but consists of three unions. Similarity is the sum of similarity by geotags, people and categories */
    $relatedStoriesResult = mysql_query(
      "select 
          s.StoryID as StoryID,
          Title,
          CustomTitle,
          ShortDate(StartTime) as StartTimeShort,
          ceil(exp(Importance)) as Popularity
      from (
          select
              StoryID, sum(Hits) as Hits
          from (
              select
                  s.StoryID, sum(1 / (1 + 5 * (abs(tags.Longitude - t.Longitude) + abs(tags.Latitude-t.Latitude)))) as Hits
              from
                  (
                      select StoryID, Longitude, Latitude, Longitude-0.2 as MinLon, Longitude+0.2 as MaxLon, Latitude-0.2 as MinLat, Latitude+0.2 as MaxLat
                      from Story natural join StoryLocationTag
                      where Story.StoryID = $storyID
                  ) tags
                  join StoryLocationTag t on t.Longitude between MinLon and MaxLon and t.Latitude between MinLat and MaxLat and t.StoryID!=tags.StoryID
                  join Story s on s.StoryID=t.StoryID
                  left join PendingStoryMerges psm on psm.StoryID2 = s.StoryID
                  where psm.StoryID2 is null
              group by s.StoryID
              union
              select
                  s.StoryID, count(*) as Hits
              from
                  (
                      select StoryID, InfoEntityID
                      from Story natural join StoryInfoEntityTag
                      where Story.StoryID = $storyID
                  ) tags
                  join StoryInfoEntityTag t on t.InfoEntityID = tags.InfoEntityID and t.StoryID!=tags.StoryID
                  join Story s on s.StoryID=t.StoryID
                  left join PendingStoryMerges psm on psm.StoryID2 = s.StoryID
                  where psm.StoryID2 is null
              group by s.StoryID
              union
              select
                  s.StoryID, 0.6 * count(*) as Hits
              from
                  (
                      select StoryID, InfoCategoryID
                      from Story natural join StoryInfoCategoryTag
                      where Story.StoryID = $storyID
                  ) tags
                  join StoryInfoCategoryTag t on t.InfoCategoryID = tags.InfoCategoryID and t.StoryID!=tags.StoryID
                  join Story s on s.StoryID=t.StoryID
                  left join PendingStoryMerges psm on psm.StoryID2 = s.StoryID
                  where psm.StoryID2 is null
              group by s.StoryID
          ) T
          group by 1 order by 2 desc
          limit 10
      ) T
      natural join Story s
      left join StoryCustomTitle sct on sct.StoryID=s.StoryID;", $db_conn);
    
    if (mysql_num_rows($relatedStoriesResult) > 0) {
      $story['relatedStories'] = array();
      
      while($row = mysql_fetch_array($relatedStoriesResult)) {
        $relStory = array();
        $relStory['storyID'] = $row['StoryID'];
        if (is_null($row['CustomTitle']))
          $relStory['title'] = $row['Title'];
        else
          $relStory['title'] = urldecode($row['CustomTitle']);
        $relStory['popularity'] = $row['Popularity'];
        $relStory['startTimeShort'] = $row['StartTimeShort'];
    
        $story['relatedStories'][] = $relStory;
      }
    }
    
    $duplicateStoriesResult = mysql_query(
      "select 
        s.StoryID as StoryID,
        Title,
        CustomTitle,
        ShortDate(StartTime) as StartTimeShort,
        ceil(exp(Importance)) as Popularity
      from (
        select
            StoryID2 as StoryID, 
            count(*)/(TagCount1+TagCount2) as Hits
        from (
            select
                ActiveStory.StoryID as StoryID1,
                sikt2.StoryID as StoryID2,
                sikt2.UserCount as UserCount2,
                TagCount1,
                TagCount2,
                sikt1.InfoKeywordID
            from
                (
                    select s.StoryID, count(*) as TagCount1
                    from Story s
                    left join StoryInfoKeywordTag t on t.StoryID=s.StoryID
                    where s.StoryID = $storyID
                    group by s.StoryID
                ) ActiveStory
                join StoryInfoKeywordTag sikt1 on sikt1.StoryID=ActiveStory.StoryID
                join (
                    select StoryID, UserCount, InfoKeywordID, TagCount2
                    from
                        StoryInfoKeywordTag t
                        natural join
                        (
                            select 
                                Story.StoryID, 
                                Story.UserCount, 
                                (select count(*) from StoryInfoKeywordTag t where t.StoryID=Story.StoryID) as TagCount2
                            from (select StoryID, StartTime-interval 24 hour t1, EndTime+interval 6 hour t2 from Story where StoryID=$storyID) T,
                                Story
                                left join PendingStoryMerges psm on psm.StoryID2 = Story.StoryID
                            where StartTime between t1 and t2 
                                and Story.StoryID != T.StoryID
                                and psm.StoryID2 is null
                            order by Importance desc
                            limit 3000
                        ) NearTimeStoryIDs
                    ) sikt2 on sikt2.InfoKeywordID=sikt1.InfoKeywordID
        ) T
        group by StoryID1, StoryID2
        order by Hits desc
        limit 10
      ) T
      natural join Story s
      left join StoryCustomTitle sct on sct.StoryID=s.StoryID;", $db_conn);

    if (mysql_num_rows($duplicateStoriesResult) > 0) {
      $story['duplicateStories'] = array();
      
      while($row = mysql_fetch_array($duplicateStoriesResult)) {
        $relStory = array();
        $relStory['storyID'] = $row['StoryID'];
        if (is_null($row['CustomTitle']))
          $relStory['title'] = $row['Title'];
        else
          $relStory['title'] = urldecode($row['CustomTitle']);
        $relStory['popularity'] = $row['Popularity'];
        $relStory['startTimeShort'] = $row['StartTimeShort'];
    
        $story['duplicateStories'][] = $relStory;
      }
    }
  }

  return $story;
} //End of get_story_content()

?>