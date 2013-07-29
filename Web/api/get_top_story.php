<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$story = null;
try {
  include 'database.php';
  $conn = get_mysql_connection();

$sql =
  "select StoryID, Title, StartTime from Story
  left join StoryMerges m on m.StoryID2=Story.StoryID
  where m.StoryID1 is null and not IsHidden
  order by TopUserCountRecent desc, WeightedSizeRecent desc
  limit 1";

  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["start_time"] = $row->StartTime;
    $story["title"] = $row->Title;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($story);
?>