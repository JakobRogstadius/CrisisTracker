<?php
include('header_start.php');
include('header_end.php');

$storyID1 = -1;
if (isset($_POST['storyid1'])) {
  $storyID1 = intval($_POST['storyid1']);
}
$storyID2 = -1;
if (isset($_POST['storyid2'])) {
  $storyID2 = intval($_POST['storyid2']);
}

if ($storyID1 < 0 || $storyID1==$storyID2) {
  die('parameter error');
}

if ($storyID1 > $storyID2) {
  $tmp = $storyID1;
  $storyID1 = $storyID2;
  $storyID2 = $tmp;
}

include('api/open_db.php');
mysql_query("insert ignore into PendingStoryMerges (StoryID1, StoryID2) values ($storyID1, $storyID2);", $db_conn);
include('api/close_db.php');
?>

<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Thank you</h1>
    <p>The stories will be merged shortly.</p>
    <p><a href="story.php?storyid=<?php echo $storyID1; ?>">Return to story</a></p>
  </div>
</div>
<?php
include('footer.php');
?>