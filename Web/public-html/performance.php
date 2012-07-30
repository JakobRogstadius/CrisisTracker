<?php
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

$tweetCountResult = mysql_query("select ShortDate(DateHour) DateHourStr, TweetCount from HourStatistics where DateHour > utc_timestamp() - interval 14 day order by DateHour", $db_conn);
$tweetCount = array();
while($row = mysql_fetch_array($tweetCountResult)) {
    $tweetCount[] = $row['DateHourStr'];
    $tweetCount[] = $row['TweetCount'];
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
	    <div id="tagged_tweets_chart_div" style="width: 100%; height: 200px;"></div>
	    <div id="tagged_stories_chart_div" style="width: 100%; height: 200px;"></div>
	    <div id="tweet_count_chart_div" style="width: 100%; height: 200px;"></div>
        </div>
    
	<h1>Tracking keywords</h1>
	<div class="gui-subpanel">
	    <p>The following keywords are currently being tracked on Twitter.</p>
	    <table class="normal-table">
	      <tr><td><strong>Keyword</strong></td><td><strong>Approximate daily hits</strong></td></tr>
		<?php
		$query = "select Word, round(Hits1d) Hits from TwitterTrackFilter where FilterType=0 and IsActive order by Hits1d desc;";
		$result = mysql_query($query, $db_conn);
		while($row = mysql_fetch_array($result)) {
		  echo '<tr><td>' . $row['Word'] . '</td><td>' . $row['Hits'] . '</td></tr>';
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