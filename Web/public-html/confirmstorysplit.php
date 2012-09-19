<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
?>
<link rel="stylesheet" href="resources/css/story.css" type="text/css" media="screen" />
<?php
include('header_end.php');
include('api/get_story_content.php');
include('api/open_db.php');

$storyID1 = 0;
if (isset($_GET['storyid'])) {
  $storyID = intval($_GET['storyid']);
}
$storyID2 = 0;
if (isset($_GET['tweetclusterid'])) {
  $tweetClusterID = intval($_GET['tweetclusterid']);
}
?>

<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Confirm splitting story</h1>
    <p>The system sometimes groups together information which doesn't belong together into a single story. By splitting such stories, you can help to better organize the information in the system.</p>
    <p>This action cannot easily be undone.</p>
    <form action="splitstory.php" method="post">
      <input type="hidden" name="storyid" value="<?php echo $storyID; ?>" />
      <input type="hidden" name="tweetclusterid" value="<?php echo $tweetClusterID; ?>" />
      <input type="button" value="Cancel" onclick="history.back();" />
      <input type="submit" value="Split this story" />
    </form>
  </div>
</div>

<?php
//Fetch story content
$story = get_story_content($storyID, 'size', $db_conn, false);

//Fetch cluster content
$cluster = array('tweetClusterID' => $tweetClusterID);
$clusterInfoResult = mysql_query("select Title from TweetCluster where TweetClusterID = $tweetClusterID", $db_conn);
$clusterInfo = mysql_fetch_array($clusterInfoResult);
$cluster['title'] = $clusterInfo['Title'];

//Summary of tweets
$clusterSummaryResult = mysql_query(
"select * from (
    select T.TweetCount, t.TweetID, t.Text, t.CreatedAt, ShortDate(t.CreatedAt) as CreatedAtShort, u.ScreenName, u.RealName, u.UserID
    from (
        select min(TweetID) as FirstID, count(*) as TweetCount from Tweet where TweetClusterID=$tweetClusterID
        group by lcase(IF(left(Text, 30) REGEXP '^RT @[a-zA-Z0-9_]+: ', SUBSTR(Text, LOCATE(':', Text) + 2, 30), left(Text, 30)))
    ) T
    join Tweet t on t.TweetID = T.FirstID
    join TwitterUser u on u.UserID = t.UserID
    where Text!=''
    order by TweetCount desc
    limit 50) T2
order by TweetCount desc", $db_conn);

if (mysql_num_rows($clusterSummaryResult) > 0) {
  $cluster['topTweets'] = array();
  
  while($row = mysql_fetch_array($clusterSummaryResult)) {
    $tweetGroup = array();
    $tweetGroup['firstCreatedAt'] = $row['CreatedAt'];
    $tweetGroup['firstCreatedAtShort'] = $row['CreatedAtShort'];
    $tweetGroup['firstTweetID'] = $row['TweetID'];
    $tweetGroup['firstUserID'] = $row['UserID'];
    $tweetGroup['firstScreenName'] = htmlspecialchars($row['ScreenName']);
    $tweetGroup['firstRealName'] = htmlspecialchars($row['RealName']);
    $tweetGroup['count'] = $row['TweetCount'];
    $tweetGroup['text'] = $row['Text'];

    $cluster['topTweets'][] = $tweetGroup;
  }
}

include('api/close_db.php');

//Print story and cluster content
?>

<div class="left-column-half">
  <div class="gui-panel">
    <div class="gui-subpanel story-summary">
      <h1><?php echo $story['title']; ?></h1>
      <ol>
<?php
    foreach($story['topTweets'] as $tweet) {
        echo '<li><div class="story-list-item">';
        echo '<div class="info">';
        echo '<div class="sort-field">' . $tweet['count'] . '</div>';
        echo '<div class="sub-field">' . $tweet['firstCreatedAtShort'] . '</div>';
        echo '</div>';
        echo '<div class="content">';
        echo '<div class="story-title">' . addLinksToText($tweet['text']) . '</div>';
        echo '<div class="story-footer">First posted ' . $tweet['firstCreatedAt'] . ' by ' . $tweet['firstRealName'] . ' (<a href="http://twitter.com/#!/' . $tweet['firstScreenName'] . '/status/' . $tweet['firstTweetID'] . '" target="_blank">@' . $tweet['firstScreenName'] . '</a>)</div>';
        echo '</div>';
        echo '</div></li>';
    }
?>
      </ol>
    </div>
  </div>
</div>

<div class="right-column-half">
  <div class="gui-panel">
    <div class="gui-subpanel story-summary">
      <h1>These tweets will be removed from the story</h1>
      <ol>
<?php
    foreach($cluster['topTweets'] as $tweet) {
        echo '<li><div class="story-list-item">';
        echo '<div class="info">';
        echo '<div class="sort-field">' . $tweet['count'] . '</div>';
        echo '<div class="sub-field">' . $tweet['firstCreatedAtShort'] . '</div>';
        echo '</div>';
        echo '<div class="content">';
        echo '<div class="story-title">' . addLinksToText($tweet['text']) . '</div>';
        echo '<div class="story-footer">First posted ' . $tweet['firstCreatedAt'] . ' by ' . $tweet['firstRealName'] . ' (<a href="http://twitter.com/#!/' . $tweet['firstScreenName'] . '/status/' . $tweet['firstTweetID'] . '" target="_blank">@' . $tweet['firstScreenName'] . '</a>)</div>';
        echo '</div>';
        echo '</div></li>';
    }
?>
      </ol>
    </div>
  </div>
</div>        

<?php
include('footer.php');
?>