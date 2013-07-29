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

include('api/open_db.php');

//Percent of tweets tagged
$taggedResult = mysql_query(
"select 
    date(StartTime) Date,
    date_format(date(StartTime), '%b %d') DateStr,
    100 * sum(if(TagScore>0, TweetCount+RetweetCount, 0)) / sum(TweetCount+RetweetCount) TaggedRatio,
    sum(TagScore>0) as TaggedCount
from Story 
where UserCount>1 and not IsHidden and StartTime > utc_date() - interval 30 day
group by 1", $db_conn);

$taggedInfo = array();
while($row = mysql_fetch_array($taggedResult)) {
    $day = array();
    $day['DateStr'] = $row['DateStr'];
    $day['TaggedRatio'] = $row['TaggedRatio'];
    $day['TaggedCount'] = $row['TaggedCount'];
    $taggedInfo[] = $day;
}

$tweetCountResult = mysql_query("select ShortDate(DateHour) DateHourStr, TweetsProcessed from HourStatistics where DateHour > utc_timestamp() - interval 14 day order by DateHour", $db_conn);
$tweetCount = array();
while($row = mysql_fetch_array($tweetCountResult)) {
    $tweetCount[] = $row['DateHourStr'];
    $tweetCount[] = $row['TweetsProcessed'];
}


?>

<script type="text/javascript" src="https://www.google.com/jsapi"></script>
<script type="text/javascript">
  google.load("visualization", "1", {packages:["corechart"]});
  google.setOnLoadCallback(drawCharts);
  
  function drawCharts() {
    drawChartTweetsTagged();
    drawChartStories();
    drawChartTweetCount();
  }
  
  function drawChartTweetsTagged() {
    var data = google.visualization.arrayToDataTable([
	['Date', '% tagged'],
<?php
    for ($i=0;$i<count($taggedInfo);$i++) {
	if ($i>0)
	    echo ",";
	echo "['" . $taggedInfo[$i]['DateStr'] . "',"
	    . $taggedInfo[$i]['TaggedRatio'] . "]";
    }
?>
    ]);

    var options = {
      title: 'Percent of tweets tagged, by start date of story (30 days)',
      legend: { position: 'none' },
      chartArea:{left:40,width:"95%"},
      colors: ['#C74451']
    };

    var chart = new google.visualization.ColumnChart(document.getElementById('tagged_tweets_chart_div'));
    chart.draw(data, options);
  }
  
  function drawChartStories() {
    var data = google.visualization.arrayToDataTable([
	['Date', 'Tagged stories'],
<?php
    for ($i=0;$i<count($taggedInfo);$i++) {
	if ($i>0)
	    echo ",";
	echo "['" . $taggedInfo[$i]['DateStr'] . "',"
	    . $taggedInfo[$i]['TaggedCount'] . "]";
    }
?>
    ]);

    var options = {
      title: 'Number of stories tagged, by start date of story (30 days)',
      legend: { position: 'none' },
      chartArea:{left:40,width:"95%"},
      colors: ['#C74451']
    };

    var chart = new google.visualization.ColumnChart(document.getElementById('tagged_stories_chart_div'));
    chart.draw(data, options);
  }

  function drawChartTweetCount() {
    var data = google.visualization.arrayToDataTable([
	['Hour', 'Processed tweets'],
<?php
    for ($i=0;$i<count($tweetCount);$i+=2) {
	if ($i>0)
	    echo ",";
	echo "['" . $tweetCount[$i] . "'," . $tweetCount[$i+1] . "]";
    }
?>
    ]);

    var options = {
      title: 'Number of tweets processed, by hour (14 days)',
      legend: { position: 'none' },
      chartArea:{left:80,width:"92%"},
      colors: ['#C74451']
    };

    var chart = new google.visualization.ColumnChart(document.getElementById('tweet_count_chart_div'));
    chart.draw(data, options);
  }
</script>

  <div class="fullwidth-column">
    <div class="gui-panel">
    	<h1>Tagging performance</h1>
      <div class="gui-subpanel">
      <div id="tweet_count_chart_div" style="width: 100%; height: 160px;"></div>
      <div id="tagged_tweets_chart_div" style="width: 100%; height: 160px;"></div>
      <div id="tagged_stories_chart_div" style="width: 100%; height: 160px;"></div>
    </div>
    
  <h1>Top curators</h1>
  <p><i><b>Names are by default hidden. If you want others to see your name in the list, go to <a href="userprofile.php?userid=<?php echo get_user_id(); ?>">your profile page</a> and disable anonymity.</b></i></p>
  <div class="gui-subpanel">
    <table style="border-spacing: 4px; border-collapse: separate; text-align: right;">
      <tr>
        <td style="text-align: left;">&nbsp;</td>
        <td style="width: 50px;" title="Total number of stories curated">Stories</td>
        <td style="width: 50px;" title="Total curation actions">Actions</td>
        <td style="width: 50px;" title="Number of days the curator has been active">Days</td>
        <td style="width: 35px;" title="Stories merged"><img src="img/merge.png" /></td>
        <td title="Stories split"><img src="img/split.png" /></td>
        <td style="width: 35px;" title="Category tags added"><img src="img/tag_category.png" /></td>
        <td title="Category tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_category.png" /></td>
        <td style="width: 35px;" title="Location tags added"><img src="img/tag_location.png" /></td>
        <td title="Location tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_location.png" /></td>
        <td style="width: 35px;" title="Named entity tags added"><img src="img/tag_entity.png" /></td>
        <td title="Named entity tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_entity.png" /></td>
        <td style="width: 35px;" title="Keyword tags added"><img src="img/tag_keyword.png" /></td>
        <td title="Keyword tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_keyword.png" /></td>
        <td style="width: 35px;" title="Stories hidden"><img src="img/tag_trash.png" /></td>
        <td title="Stories shown"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_trash.png" /></td>
      </tr>
<?php
include('api/open_db.php');

$uid = get_user_id(TRUE);
if (is_null($uid))
  $uid = 0;
$result = mysql_query(
"select 
    StoryLog.UserID,
    if (IsAnonymous=0, User.Name, if (TwitterUserID=". $uid .", concat('Anonymous (you)'), 'Anonymous')) as Name,
    coalesce(IsAnonymous,1) as IsAnonymous,
    count(distinct StoryID) as StoryCount, 
    count(*) as ActionCount,
    count(distinct date(Timestamp)) as DaysActive,
    sum(EventType=12) as 'Merge',
    sum(EventType=13) as Split,
    sum(EventType=14) as Hide,
    sum(EventType=15) as 'Show',
    sum(EventType=30) as AddKeyword,
    sum(EventType=40) as DelKeyword,
    sum(EventType=31) as AddCategory,
    sum(EventType=41) as DelCategory,
    sum(EventType=32) as AddEntity,
    sum(EventType=42) as DelEntity,
    sum(EventType=33) as AddLocation,
    sum(EventType=43) as DelLocation    
from 
    StoryLog
    left join User on StoryLog.UserID=User.TwitterUserID
where EventType!=20 and StoryLog.UserID!=0
group by StoryLog.UserID
order by StoryCount desc, ActionCount desc
;", $db_conn);

while($row = mysql_fetch_array($result)) {
  echo '<tr>';
  if ($row['IsAnonymous'])
    echo '<td style="text-align: left;">' . $row['Name'] . '</td>';
  else
    echo '<td style="text-align: left;"><a href="userprofile?userid=' . $row['UserID'] . '">' . $row['Name'] . '</a></td>';
  echo '<td>' . $row['StoryCount'] . '</td>';
  echo '<td>' . $row['ActionCount'] . '</td>';
  echo '<td>' . $row['DaysActive'] . '</td>';
  echo '<td>' . $row['Merge'] . '</td>';
  echo '<td>' . $row['Split'] . '</td>';
  echo '<td>' . $row['AddCategory'] . '</td>';
  echo '<td>' . $row['DelCategory'] . '</td>';
  echo '<td>' . $row['AddLocation'] . '</td>';
  echo '<td>' . $row['DelLocation'] . '</td>';
  echo '<td>' . $row['AddEntity'] . '</td>';
  echo '<td>' . $row['DelEntity'] . '</td>';
  echo '<td>' . $row['AddKeyword'] . '</td>';
  echo '<td>' . $row['DelKeyword'] . '</td>';
  echo '<td>' . $row['Hide'] . '</td>';
  echo '<td>' . $row['Show'] . '</td>';
  echo '</tr>';
}
?>
      </table>
    </div>
  
  	<h1>Tracking keywords</h1>
  	<div class="gui-subpanel">
      <p>The following keywords are currently being tracked on Twitter.</p>
      <table class="normal-table">
        <tr><td><strong>Keyword</strong></td><td><strong>Approximate daily tweets</strong></td><td><strong>Discarded tweets</strong></td></tr>
        <?php
        $query = "select Word, round(Hits1d) Hits, round(100*Discards1d/Hits1d) as Discarded from TwitterTrackFilter where FilterType=0 and IsActive order by Hits1d desc;";
        $result = mysql_query($query, $db_conn);
        while($row = mysql_fetch_array($result)) {
          echo '<tr><td>' . $row['Word'] . '</td><td>' . $row['Hits'] . '</td><td>' . $row['Discarded'] . '%</td></tr>';
        }
        ?>
	    </table>
    </div>
  </div>
</div>


<?php
include('api/close_db.php');
include('footer.php');
?>