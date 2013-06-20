<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $mintime = getTimeParam("mintime");
  $maxtime = getTimeParam("maxtime");
  $textfilter = getSafeTextParam("text", $conn);
  $fetchOngoingStories = getIntParam("ongoing", 0, 1, 1);

  $where = '';
  $having = '';
  $join = '';
  if (!$fetchOngoingStories && $mintime != null) {
    $where .= "Story.StartTime >= '$mintime' ";
  }
  if (!$fetchOngoingStories && $maxtime != null) {
    if (strlen($where) > 0)
      $where .= ' and ';
    $where .= "Story.StartTime <= '$maxtime' ";
  }
  if ($textfilter != null) {
    if (strlen($where) > 0)
      $where .= ' and ';
    $words = stemMany(preg_split("/,| |;/", $textfilter));
    $textfilter = "'" . implode("','", $words) . "'";
    $where .= "Keyword in ($textfilter) ";
    $join =
      "join StoryInfoKeywordTag sikt on sikt.StoryID=Story.StoryID
      join InfoKeyword ik on ik.InfoKeywordID=sikt.InfoKeywordID";
    $having = "having count(distinct ik.InfoKeywordID)=" . sizeof($words);

  }
  if (strlen($where) > 0)
    $where = 'where ' . $where;

  $orderByInner = ($fetchOngoingStories
              ? "TopUserCountRecent desc, WeightedSizeRecent desc"
                : "TopUserCount desc, StartTime desc");
  //$orderByOuter = ($fetchOngoingStories ? "" : "order by StartTime desc");
  $orderByOuter = "order by StartTime desc";

  $sql =
      "select * from (
        select
          Story.StoryID,
          round(WeightedSize) WeightedSize,
          StartTime,
          ShortDate(StartTime) as StartTimeShort,
          Title
        from Story $join
        $where
        group by 1
        $having
        order by $orderByInner
        limit 20) T
      $orderByOuter";

  $conn = get_mysql_connection();
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  $stories = array();

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["weighted_size"] = $row->WeightedSize;
    $story["start_time"] = $row->StartTime;
    $story["start_time_short"] = $row->StartTimeShort;
    $story["title"] = $row->Title;
    $story["tweet_trend"] = $row->TweetTrend;
    $story["retweet_trend"] = $row->RetweetTrend;
    $stories[] = $story;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();
echo json_encode($stories);
?>