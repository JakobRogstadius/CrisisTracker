<?php
include('header_start.php');
include('header_end.php');

$story = -1;
if (isset($_POST['storyid'])) {
  $storyID = intval($_POST['storyid']);
}
$tweetClusterID = -1;
if (isset($_POST['tweetclusterid'])) {
  $tweetClusterID = intval($_POST['tweetclusterid']);
}

if ($storyID < 0 || $tweetClusterID < 0) {
  die('parameter error');
}

include('api/open_db.php');
mysql_query("insert ignore into PendingStorySplits (StoryID, TweetClusterID) values ($storyID, $tweetClusterID);", $db_conn);
include('api/close_db.php');
?>

<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Thank you</h1>
    <p>The story will be split shortly.</p>
    <p><a href="story.php?storyid=<?php echo $storyID; ?>">Return to story</a></p>
  </div>
</div>
<?php
include('footer.php');
?>