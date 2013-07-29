<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
include('header_end.php');

if(is_logged_in()) {
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

  $ip = $_SERVER['REMOTE_ADDR'];
  $userID = get_user_id();

  include('api/open_db.php');
  mysql_query("insert ignore into StorySplits (StoryID, TweetClusterID, IP, UserID) values ($storyID, $tweetClusterID, INET_ATON('$ip'), $userID);", $db_conn);
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
}
else {
  echo '<div class="fullwidth-column"><div class="gui-panel"><p>You must log in before you can split stories.</p></div></div>';
}
include('footer.php');
?>