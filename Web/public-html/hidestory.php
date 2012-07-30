<?php
//include('header_start.php');
//include('header_end.php');

$storyID = -1;
if (isset($_GET['storyid'])) {
  $storyID = intval($_GET['storyid']);
}

if ($storyID < 0) {
  die('parameter error');
}

include('api/open_db.php');
mysql_query("update Story set IsHidden=1-IsHidden where StoryID=$storyID;", $db_conn);
include('api/close_db.php');

//header('Location: story.php?storyid=' . $storyID);
header('Location: ' . $_SERVER['HTTP_REFERER'] );

//include('footer.php');
?>