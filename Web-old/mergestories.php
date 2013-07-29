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

  $ip = $_SERVER['REMOTE_ADDR'];
  $userID = get_user_id();

  include('api/open_db.php');
  mysql_query("insert ignore into StoryMerges (StoryID1, StoryID2, IP, UserID) values ($storyID1, $storyID2, INET_ATON('$ip'), $userID);", $db_conn);
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
}
else {
  echo '<div class="fullwidth-column"><div class="gui-panel"><p>You must log in before you can merge stories.</p></div></div>';
}

include('footer.php');
?>