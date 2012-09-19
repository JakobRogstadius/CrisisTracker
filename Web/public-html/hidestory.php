<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/
 
 include('twitterLogin/login.php');
if (isLoggedIn()) {
  $storyID = 0;
  if (isset($_GET['storyid']))
    $storyID = intval($_GET['storyid']);
  else 
    die('parameter error');
  
  $hidden = 1;
  if (isset($_GET['hidden'])) {
    $hidden = intval($_GET['hidden']);
    if ($hidden > 1 || $hidden < 0)
      die('parameter error');
  }
  else 
    die('parameter error');
  
  $ip = $_SERVER['REMOTE_ADDR'];
  $userID = getUserID();
  
  include('api/open_db.php');
  mysql_query("call HideShowStory($storyID, $hidden, INET_ATON('$ip'), $userID);", $db_conn);
  include('api/close_db.php');
}
header('Location: ' . $_SERVER['HTTP_REFERER'] );
?>