<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $id = intval($_REQUEST['id']);
  $mintime = getTimeParam("mintime");
  $maxtime = getTimeParam("maxtime");
  $textfilter = getSafeTextParam("text", $conn);

$where = '';
if ($textfilter != null)
  $where = "and Text like '%$textfilter%'";
$having = '';
if ($maxtime != null && $mintime != null)
  $having = "and FirstSeen between '$mintime' and '$maxtime'";

$sql =
  "select * from (
    select
        min(TweetID) as TweetID, min(CreatedAt) as FirstSeen, count(distinct UserID) as UserCount, log(1+count(distinct UserID))*pow(max(Novelty),2) as OrderByWeight
    from Tweet
        natural join TweetCluster
        natural join TwitterUser
    where StoryID=$id and not IsBlacklisted $where
    group by TextHash
    having max(Novelty) > 0.2 $having
    order by OrderByWeight desc limit 20
  ) T
  natural join Tweet
  natural join TwitterUser
  order by OrderByWeight desc";

  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  while($row = $result->fetch_object()) {
    $tweet = array();
    $tweet["tweet_id"] = $row->TweetID;
    $tweet["user_id"] = $row->UserID;
    $tweet["user_real_name"] = $row->RealName;
    $tweet["user_screen_name"] = $row->ScreenName;
    $tweet["profile_image_url"] = $row->ProfileImageUrl;
    $tweet["created_at"] = $row->CreatedAt;
    $tweet["created_at_short"] = $row->CreatedAtShort;
    $tweet["user_count"] = $row->UserCount;
    $tweet["text"] = $row->Text;
    $tweets[] = $tweet;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($tweets);
?>