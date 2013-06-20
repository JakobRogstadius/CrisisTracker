<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

header('Access-Control-Allow-Origin: *');
include('common.php');

if(isLoggedIn()) {
  $storyID1 = -1;
  if (isset($_REQUEST['storyid1'])) {
    $storyID1 = intval($_REQUEST['storyid1']);
  }
  $storyID2 = -1;
  if (isset($_REQUEST['storyid2'])) {
    $storyID2 = intval($_REQUEST['storyid2']);
  }

  if ($storyID1 < 0 || $storyID1==$storyID2) {
    die('0');
  }

  if ($storyID1 > $storyID2) {
    $tmp = $storyID1;
    $storyID1 = $storyID2;
    $storyID2 = $tmp;
  }

  $ip = $_SERVER['REMOTE_ADDR'];
  $userID = getUserID();

  include 'database.php';
  $conn = get_mysql_connection();
  $conn->query("insert ignore into StoryMerges (StoryID1, StoryID2, IP, UserID) values ($storyID1, $storyID2, INET_ATON('$ip'), $userID)");
  $conn->close();
  echo 1;
}
else {
  echo 0;
}
?>