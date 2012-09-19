<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

function was_story_merged($storyID, $db_conn) {
  $result = mysql_query("select StoryID1 from PendingStoryMerges where StoryID2=$storyID", $db_conn);
  $row = mysql_fetch_array($result);  
  if (is_null($row))
    return null;
  else
    return $row['StoryID1'];
}

?>