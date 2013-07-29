<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$story = null;
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $id = intval($_REQUEST['id']);

$sql =
  "select (select StoryID from Story where StoryID=$id) StoryID,
  (select StoryID1 from StoryMerges where StoryID2=$id) MergedWith";

  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["merged_with"] = $row->MergedWith;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($story);
?>