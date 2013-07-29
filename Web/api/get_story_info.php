<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$story = null;
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $id = intval($_REQUEST['id']);

  $sql = "select * from Story where StoryID=$id";

  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["tweet_count"] = $row->TweetCount;
    $story["retweet_count"] = $row->RetweetCount;
    $story["user_count"] = $row->UserCount;
    $story["top_user_count"] = $row->TopUserCount;
    $story["top_user_count_recent"] = $row->TopUserCountRecent;
    $story["weighted_size"] = $row->WeightedSize;
    $story["weighted_size_rencent"] = $row->WeightedSizeRecent;
    $story["start_time"] = $row->StartTime;
    $story["end_time"] = $row->EndTime;
    $story["is_archived"] = $row->IsArchived;
    $story["is_hidden"] = $row->IsHidden;
    $story["latitude"] = $row->Latitude;
    $story["longitude"] = $row->Longitude;
    $story["title"] = $row->Title;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($story);
?>