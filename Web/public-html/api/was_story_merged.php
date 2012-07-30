<?php

function was_story_merged($storyID, $db_conn) {
  $result = mysql_query("select StoryID1 from PendingStoryMerges where StoryID2=$storyID", $db_conn);
  $row = mysql_fetch_array($result);  
  if (is_null($row))
    return null;
  else
    return $row['StoryID1'];
}

?>