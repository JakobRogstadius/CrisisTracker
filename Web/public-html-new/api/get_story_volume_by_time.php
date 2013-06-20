<?php
header('Access-Control-Allow-Origin: *');

$id = intval($_REQUEST["id"]);
include 'database.php';

/*
$sql =
"select
  from_unixtime(dT*floor(least(MaxTime, unix_timestamp(CreatedAt))/dT)) CreatedAt,
  count(distinct if(IsDuplicate, null, TextHash)) TweetCount,
  count(*) RetweetCount
from
	(select
	  unix_timestamp(EndTime)-2 MaxTime, greatest(
		300,
		(unix_timestamp(EndTime)-unix_timestamp(StartTime))/500) dT
	from Story where StoryID=$id) intvars,
	Tweet
	natural join TweetCluster
	natural join TwitterUser
where StoryID=$id and not IsBlacklisted
group by 1";
*/
/*
$sql =
"select CreatedAt, sum(TweetCount) as TweetCount from (
select
  TextHash,
  from_unixtime(dT*floor(least(MaxTime, unix_timestamp(min(CreatedAt)))/dT)) CreatedAt,
  count(*) as TweetCount
from
	(select
	  unix_timestamp(EndTime)-2 MaxTime, greatest(
		300,
		(unix_timestamp(EndTime)-unix_timestamp(StartTime))/500) dT
	from Story where StoryID=$id) intvars,
	Tweet
	natural join TweetCluster
	natural join TwitterUser
where StoryID=$id and not IsBlacklisted
group by 1
) T
group by 1
";
*/

$sql =
"select CreatedAt, sum(TweetsFromT) FirstSeenAt, sum(TweetCount) TweetCount from (
	select CreatedAt, sum(TweetsFromT) as TweetsFromT, 0 as TweetCount from (
	select
	  TextHash,
	  from_unixtime(dT*floor(least(MaxTime, unix_timestamp(min(CreatedAt)))/dT)) CreatedAt,
	  count(*) as TweetsFromT
	from
		(select
		  unix_timestamp(EndTime)-2 MaxTime, greatest(
			300,
			(unix_timestamp(EndTime)-unix_timestamp(StartTime))/400) dT
		from Story where StoryID=$id) intvars,
		Tweet
		natural join TweetCluster
		natural join TwitterUser
	where StoryID=$id and not IsBlacklisted
	group by 1
	) T
	group by 1

	union

	select
	  from_unixtime(dT*floor(least(MaxTime, unix_timestamp(CreatedAt))/dT)) CreatedAt,
	  0 as TweetsFromT,
      count(*) TweetCount
	from
		(select
		  unix_timestamp(EndTime)-2 MaxTime, greatest(
			300,
			(unix_timestamp(EndTime)-unix_timestamp(StartTime))/400) dT
		from Story where StoryID=$id) intvars,
		Tweet
		natural join TweetCluster
		natural join TwitterUser
	where StoryID=$id and not IsBlacklisted
	group by 1
) T
group by 1
";

$conn = get_mysql_connection();
$conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
$result = $conn->query($sql);
$conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

$data = array();
while($row = $result->fetch_object()) {
  $item = array();
  $item["created_at"] = $row->CreatedAt;
  $item["first_seen_at"] = intval($row->FirstSeenAt);
  $item["tweet_count"] = intval($row->TweetCount);
  $data[] = $item;
}

$conn->close();

echo json_encode($data);
?>