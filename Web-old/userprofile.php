<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

function printJSDataTable($varName, $sqlResult, $colNames) {
  echo "var $varName = google.visualization.arrayToDataTable([";
  echo "['" . implode("','", $colNames) . "']";
  while($row = mysql_fetch_array($sqlResult)) {
    echo ",[";
    for ($i=0; $i<count($colNames); $i++) {
      $name = $colNames[$i];
      if ($i > 0)
        echo ',';
      if (is_numeric($row[$name]))
        echo $row[$name];
      else
        echo "'" . $row[$name] . "'";
    }
    echo "]";
  }
  echo "]);";
}

include('twitteroauth/login.php');
$loggedInUserID = get_user_id();

$pageUserID = null;
if (isset($_GET['userid'])) {
  $pageUserID = intval($_GET['userid']);
}
if (is_null($pageUserID)) {
  exit;
}

include('api/open_db.php');

//GET DATA
$isAnonResult = mysql_query("select Name, if(IsAnonymous,1,0) as IsAnonymous from User where TwitterUserID=" . $pageUserID . ";", $db_conn);
$row = mysql_fetch_object($isAnonResult);
$isAnon = $row->IsAnonymous;
$userName = $row->Name;

if (is_null($userName) || ($isAnon && $loggedInUserID != $pageUserID)) //Can't peek at anon's profile
  exit;

$curationCount = mysql_query(
  "select count(distinct StoryID) as StoryCount, count(*) as ActionCount
  from StoryLog
  where UserID=$pageUserID and EventType!=20;", $db_conn);

// Pie chart of user activity (event types)
$userActivityByEventType = mysql_query(
  "select EventType, Name as 'Type of curation', count(*) as 'Number of stories'
  from StoryLog natural join StoryLogEventType
  where UserID=$pageUserID and EventType!=20
  group by 1 order by 3 desc;", $db_conn);

// Stories recently modified by user
$recentStories = mysql_query(
  "select T.StoryID, Title, Name
  from (
      select coalesce(MergedWithStoryID, StoryID) as StoryID, group_concat(DISTINCT Name ORDER BY EventType DESC SEPARATOR ', ') as Name
      from StoryLog natural join StoryLogEventType
      where UserID=$pageUserID and (EventType between 10 and 19 or EventType between 30 and 49)
      group by 1
      order by LogID desc
      limit 20
  ) T
  join Story on Story.StoryID = T.StoryID;", $db_conn);

// Stories modified by user, per day
$userActivityByDay = mysql_query("select date(Timestamp) as Date, count(distinct StoryID) as 'Number of stories' from StoryLog where UserID=$pageUserID and EventType!=20 group by 1;", $db_conn);

include('header_start.php');
?>

<script type="text/javascript" src="https://www.google.com/jsapi"></script>
<script>
  google.load("visualization", "1", {packages:["corechart"]});
  google.setOnLoadCallback(drawCharts);

  function drawCharts() {
    drawUserStoriesPerDayChart();
    drawUserActivityByEventTypeChart();
  }

  function drawUserStoriesPerDayChart() {
    <?php printJSDataTable('data', $userActivityByDay, array('Date', 'Number of stories')); ?>

    var options = {
      title: 'Number of stories curated per day (30 days)',
      legend: { position: 'none' },
      chartArea:{left:40,width:"95%"},
      colors: ['#C74451']
    };

    var chart = new google.visualization.ColumnChart(document.getElementById("userActivityByDayChart"));
    chart.draw(data, options);
  }

  function drawUserActivityByEventTypeChart() {
    <?php printJSDataTable('data', $userActivityByEventType, array('Type of curation', 'Number of stories')); ?>

    /*var options = {
      title: 'Number of stories curated per day (30 days)',
      legend: { position: 'none' },
      chartArea:{left:40,width:"95%"},
      colors: ['#C74451']*/

    var options = {
      title: 'Breakdown of curation, by number of stories'
    };

    var chart = new google.visualization.PieChart(document.getElementById('userActivityByEventTypeChart'));
    chart.draw(data, options);
  }
</script>

<?php
include('header_end.php');
?>

<div class="fullwidth-column">
  <div class="gui-panel">
	<h1><?php echo $userName; ?></h1>
  <?php
  if ($loggedInUserID == $pageUserID) {
    $confirmText = ($isAnon ? "Reveal your name to other visitors of the website?" : "Hide your name from other visitors of the website?");
    echo '<p><a class="actionlink' . ($isAnon ? ' actionlink-highlight' : '') .'"';
    echo ' title="Toggle whether your username is visible to other visitors of the website, or if you should be listed as \'Anonymoys\'"';
    echo ' href="#"';
    echo ' onclick="if(confirm(\'' . $confirmText . '\')) window.location=\'api/update_user_profile.php?isanonymous=' . ($isAnon ? 0 : 1) . '\'">Anonymity is ' . ($isAnon ? "ON" : "OFF") . '</a></p>';
  }
  ?>
	<div class="gui-subpanel">
    <div>
      <h2>Total curation</h2>
<?php
  $curationCountRow = mysql_fetch_array($curationCount);
  echo '<p>Total stories curated: ' . $curationCountRow["StoryCount"] . '</p>';
  echo '<p>Total curation actions: ' . $curationCountRow["ActionCount"] . '</p>';
?>
    </div>
    <div id="userActivityByDayChart" style="width: 100%; height: 200px;"></div>
    <div id="userActivityByEventTypeChart" style="width: 100%; height: 200px;"></div>
    <div>
      <h2>Stories most recently curated</h2>
      <span class="table">
<?php
  while($row = mysql_fetch_array($recentStories)) {
    $storyID = $row["StoryID"];
    $storyTitle = $row["Title"];
    $eventType = $row["Name"];
    echo "<span class=\"tr\"><span class=\"td\"><a href=\"story.php?storyid=$storyID\">$storyTitle</a></span><span class=\"td\">$eventType</span></span>";
  }
?>
      </span>
    </div>
  </div>
</div>

<?php
include('api/close_db.php');
include('footer.php');
?>
