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

<link rel="stylesheet" href="resources/css/storylist.css" type="text/css" media="screen" />

<script type="text/javascript">
  function confirmHideStory(storyid) {
    <?php if (isLoggedIn()) { ?>
      if(confirm('Stories that do not contribute to situation awareness should be hidden. Hide this story?')) {
        window.location = "hidestory.php?hidden=1&storyid=" + storyid;
        return true;
      }
      return false;
    <?php } else { ?>
      alert('You must log in before you can hide stories.');
      return false;    
    <?php } ?>    
  }
</script>

<?php
include('header_end.php');

$sortOrder = 'weighted';
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if ($sortOrder != 'active' && $sortOrder != 'trending' && $sortOrder != 'hidden')
    $sortOrder = 'weighted';
}
?>


<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Pick a story to tag</h1>
    <p>Sort order:
    <?php if($sortOrder=='weighted') echo '<strong>Newest and largest</strong>';
      else echo '<a href="?sortorder=weighted">Newest and largest</a>'; ?> |
    <?php if($sortOrder=='active') echo '<strong>Most active</strong>';
      else echo '<a href="?sortorder=active">Most active</a>'; ?> |
    <?php if($sortOrder=='trending') echo '<strong>Trending</strong>';
      else echo '<a href="?sortorder=trending">Trending</a>'; ?> |
    <?php if($sortOrder=='hidden') echo '<strong>Hidden</strong>';
      else echo '<a href="?sortorder=hidden">Hidden</a>'; ?>
    <div class="gui-subpanel story-list stories-to-tag-list">
      <ol>
        <li class="story-list-item header">
          <span class="column size">Popularity</span>
          <span class="column time">Time</span>
          <span class="column title">Title</span>
          <span class="column">Tags</span>
        </li>
<?php
include('api/open_db.php');

$limit = 20;
if (isset($_GET['limit'])) {
  $limit = intval($_GET['limit']);
}

if ($limit < 1 || $limit > 100)
  $limit = 20;
$orderby = '';
if ($sortOrder == 'hidden')
  $orderby = 'StartTime';
elseif ($sortOrder == 'active')
  $orderby = 'ImportanceRecent / (1 + 10*TagScore)';
elseif ($sortOrder == 'trending')
  $orderby = 'Trend / (1 + 10*TagScore)';
elseif ($sortOrder == 'recent')
  $orderby = 'StartTime';
else //weighted
  $orderby = 'TaggingImportance / log(10 + (unix_timestamp(utc_timestamp()) - unix_timestamp(StartTime)) / 60)';

$hideArabic = "";
if ($_COOKIE["hidearabic"] == "true")
  $hideArabic = "and Title not regexp '[؀-ۿ]'";

// Story info
$isHidden = ($sortOrder == 'hidden' ? 1 : 0);
$storyInfoResult = mysql_query(
"select
    s.StoryID, Title, CustomTitle, Popularity, StartTime,
    count(distinct TagID) as LocationCount,
    count(distinct InfoCategoryID) as CategoryCount,
    count(distinct InfoEntityID) as EntityCount,
    count(distinct InfoKeywordID) as KeywordCount
from (
        select 
            Story.StoryID,
            Title,
            CustomTitle,
            ceil(exp(Importance)) as Popularity,
            ShortDate(StartTime) as StartTime,
            $orderby as score
        from Story
            left join PendingStoryMerges psm on psm.StoryID2 = Story.StoryID
            left join StoryCustomTitle sct on sct.StoryID=Story.StoryID
        where psm.StoryID2 is null and IsHidden=$isHidden $hideArabic
        order by score desc
        limit $limit
    ) s
    left join StoryInfoCategoryTag c on c.StoryID=s.StoryID
    left join StoryInfoEntityTag e on e.StoryID=s.StoryID
    left join StoryInfoKeywordTag k on k.StoryID=s.StoryID
    left join StoryLocationTag l on l.StoryID=s.StoryID
group by s.StoryID
order by score desc", $db_conn);

while($row = mysql_fetch_array($storyInfoResult)) {
    echo '<li class="story-list-item">';
    echo '<span class="column hidestory"';
    echo ' title="Hide this story if it does not contribute to situation awareness. Hiding the story excludes it from searches in CrisisTracker."';
    echo ' onclick="confirmHideStory(' . $row['StoryID'] . ')"><img style="margin-top:2px;" src="img/tag_trash.png" alt="Hide story"/></span>';

    echo '<a class="wrapper" href="story.php?storyid=' . $row['StoryID'] . '">';
    echo '<span class="column size">' . $row['Popularity'] . '</span>';
    echo '<span class="column time">' . $row['StartTime'] . '</span>';
    if (is_null($row['CustomTitle']))
        echo '<span class="column title">' . htmlspecialchars($row['Title']) . '</span>';
    else
        echo '<span class="column title">' . htmlspecialchars(urldecode($row['CustomTitle'])) . '</span>';
    echo '<span class="column loc-tags'.  ($row['LocationCount'] != '0' ? ' hasvalue' : '') .'">' . $row['LocationCount'] . '</span>';
    echo '<span class="column ent-tags'.  ($row['EntityCount'] != '0' ? ' hasvalue' : '') .'">' . $row['EntityCount'] . '</span>';
    echo '<span class="column cat-tags'.  ($row['CategoryCount'] != '0' ? ' hasvalue' : '') .'">' . $row['CategoryCount'] . '</span>';
    echo '<span class="column key-tags'.  ($row['KeywordCount'] != '0' ? ' hasvalue' : '') .'">' . $row['KeywordCount'] . '</span>';
    echo '</a>';
    echo '</li>';
}

include('api/close_db.php');
?>
            </ol>
        </div>
    </div>
</div>

<?php
include('footer.php');
?>