<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
?>
<link rel="stylesheet" href="resources/css/story.css" type="text/css" media="screen" />
<?php
include('header_end.php');
include('api/get_story_content.php');
include('api/open_db.php');

$storyID1 = 0;
if (isset($_GET['storyid1'])) {
  $storyID1 = intval($_GET['storyid1']);
}
$storyID2 = 0;
if (isset($_GET['storyid2'])) {
  $storyID2 = intval($_GET['storyid2']);
}
?>

<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Confirm merging stories</h1>
    <p>The system sometimes does not manage to group together tweets which contain the same information, resulting in duplicate stories. By merging such stories, you can help to better organize the information in the system.</p>
    <p>This action cannot be undone.</p>
    <form action="mergestories.php" method="post">
      <input type="hidden" name="storyid1" value="<?php echo $storyID1; ?>" />
      <input type="hidden" name="storyid2" value="<?php echo $storyID2; ?>" />
      <input type="button" value="Cancel" onclick="history.back();" />
      <input type="submit" value="Merge the stories" />
    </form>
  </div>
</div>

<?php
//Fetch and print tweets in story
$story1 = get_story_content($storyID1, 'size', $db_conn, false);
$story2 = get_story_content($storyID2, 'size', $db_conn, false);
include('api/close_db.php');
?>

<div class="left-column-half">
  <div class="gui-panel">
    <div class="gui-subpanel story-summary">
      <h1><?php echo $story1['title']; ?></h1>
      <ol>
<?php
    foreach($story1['topTweets'] as $tweet) {
        echo '<li><div class="story-list-item">';
        echo '<div class="info">';
        echo '<div class="sort-field">' . $tweet['count'] . '</div>';
        echo '<div class="sub-field">' . $tweet['firstCreatedAtShort'] . '</div>';
        echo '</div>';
        echo '<div class="content">';
        echo '<div class="story-title">' . addLinksToText($tweet['text']) . '</div>';
        echo '<div class="story-footer">First posted ' . $tweet['firstCreatedAt'] . ' by ' . $tweet['firstRealName'] . ' (<a href="http://twitter.com/#!/' . $tweet['firstScreenName'] . '/status/' . $tweet['firstTweetID'] . '" target="_blank">@' . $tweet['firstScreenName'] . '</a>)</div>';
        echo '</div>';
        echo '</div></li>';
    }
?>
      </ol>
    </div>
  </div>
</div>

<div class="right-column-half">
  <div class="gui-panel">
    <div class="gui-subpanel story-summary">
      <h1><?php echo $story2['title']; ?></h1>
      <ol>
<?php
    foreach($story2['topTweets'] as $tweet) {
        echo '<li><div class="story-list-item">';
        echo '<div class="info">';
        echo '<div class="sort-field">' . $tweet['count'] . '</div>';
        echo '<div class="sub-field">' . $tweet['firstCreatedAtShort'] . '</div>';
        echo '</div>';
        echo '<div class="content">';
        echo '<div class="story-title">' . addLinksToText($tweet['text']) . '</div>';
        echo '<div class="story-footer">First posted ' . $tweet['firstCreatedAt'] . ' by ' . $tweet['firstRealName'] . ' (<a href="http://twitter.com/#!/' . $tweet['firstScreenName'] . '/status/' . $tweet['firstTweetID'] . '" target="_blank">@' . $tweet['firstScreenName'] . '</a>)</div>';
        echo '</div>';
        echo '</div></li>';
    }
?>
      </ol>
    </div>
  </div>
</div>        

<?php
include('footer.php');
?>