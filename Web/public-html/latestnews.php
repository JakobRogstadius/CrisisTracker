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

function naiveStemming($words) {
  for ($i=0; $i<count($words); $i++) {
    if (strlen($words[$i]) < 4 || substr_compare($words[$i],"#",0,1) == 0)
      return str;
    $before = $words[$i];
    while (($words[$i] = preg_replace("/(es|ed|s|ing|ly|n)$/", "", $words[$i])) != $before)
      $before = $words[$i];
  }
  return $words;
}

function getSafeValues($input, $readNumbers = FALSE) {
  $output = array();
  if (is_array($input)) {
    foreach($input as $tag) {
      $tmp = '';
      if ($readNumbers)
        $tmp = intval($tag);
      else
        $tmp = stripMaliciousSql($tag);
      if ($tmp != '')
        $output[] = $tmp;
    }
  }
  return $output;
}



//WHERE
$where = "where not IsHidden and psm.StoryID2 is null ";
$wherejoins = "";
$hitcounts = "";
$having = "having ";

$storycount = '10';
if (isset($_GET['storycount'])) { $storycount = max(1,min(200, intval($_GET['storycount']))); }
$hours = '8';
if (isset($_GET['hours'])) { $hours = max(1,min(30*24, intval($_GET['hours']))); }

$where = "";
$wherejoin = "";
$hitcounts = "";
$having = "";
$useAND = 1;
//Keyword filter
if (isset($_GET['keywordfilter'])) {
  $keywords = naiveStemming(getSafeValues(explode(',', $_GET['keywordfilter'])));
  if (sizeof($keywords) > 0) {
    if (isset($_GET['keywordoperator'])) {
      if ($_GET['keywordoperator'] == 'or')
        $useAND=0;
    }

    $where .= "and InfoKeywordID in (select distinct InfoKeywordID from InfoKeyword where Keyword in ('" . strtolower(implode("','", $keywords)) . "'))";
    $wherejoin .= "join StoryInfoKeywordTag keyword on keyword.StoryID=s.StoryID";
    if ($useAND) {
      $hitcounts .= ",count(distinct keyword.InfoKeywordID) KeywordHitCount";
      $having .= "having KeywordHitCount=" . sizeof($keywords);
    }
  }
}


$sqlQuery = "select
  T.*,
  count(distinct TagID) as LocationCount,
  count(distinct InfoCategoryID) as CategoryCount,
  count(distinct InfoEntityID) as EntityCount,
  count(distinct InfoKeywordID) as KeywordCount
from (
  select
    s.StoryID,
    StartTime as StartTimeRaw,
    ShortDate(StartTime) as StartTime,
    ceil(exp(Importance)) as Popularity,
    MaxGrowth,
    Title,
    CustomTitle
    $hitcounts
  from Story s
    left join PendingStoryMerges psm on psm.StoryID2=s.StoryID
    left join StoryCustomTitle sct on sct.StoryID=s.StoryID
    $wherejoin
  where
    StartTime > utc_timestamp()-interval $hours hour
    and psm.StoryID1 is null
    $where
  group by s.StoryID
  $having
  order by MaxGrowth desc 
  limit $storycount
) T
left join StoryInfoCategoryTag c on c.StoryID=T.StoryID
left join StoryInfoEntityTag e on e.StoryID=T.StoryID
left join StoryInfoKeywordTag k on k.StoryID=T.StoryID
left join StoryLocationTag l on l.StoryID=T.StoryID
group by T.StoryID
order by StartTimeRaw desc;";

//echo $sqlQuery; exit();

?>
<div class="fullwidth-column">
  <div class="gui-panel">
    <h1>Latest news</h1>
    <form method="get">
      <p>Show
      <input name="storycount" type="text" style="width: 20px" value="<? echo $storycount; ?>" />
      stories from the past
      <input name="hours" type="text" style="width: 20px" value="<? echo $hours; ?>" />
      hours containing
      <input name="keywordoperator" id="andoperator" type="radio" value="all" <? if ($useAND) echo 'checked'; ?> /><label for="andoperator">ALL</label>
      <input name="keywordoperator" id="oroperator" type="radio" value="or" <? if (!$useAND) echo 'checked'; ?> /><label for="oroperator">ANY</label>
      of the words
      <input name="keywordfilter" type="text" style="width: 150px" value="<? if (isset($_GET['keywordfilter'])) echo $_GET['keywordfilter']; ?>" />
      <input type="Submit" value="Update" />
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
$storyInfoResult = mysql_query($sqlQuery, $db_conn);

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

if (mysql_num_rows($storyInfoResult) == 0) {
  echo '<li class="story-list-item"><span class="column">No stories matched your filter settings.</span></li>';
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