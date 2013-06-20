<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';
include 'database.php';

$d = getIntParam("days", 1, 5*365, 28);
$h = 1;
if ($d > 400) $h = 48;
elseif ($d > 200) $h = 24;
elseif ($d > 100) $h = 12;
elseif ($d > 67) $h = 6;
elseif ($d > 50) $h = 4;
elseif ($d > 33) $h = 3;
elseif ($d > 17) $h = 2;

$sql =
  "select from_unixtime((3600*$h)*floor(unix_timestamp(DateHour)/(3600*$h))) DateHour, sum(TweetsProcessed - TweetsDiscarded) as Tweets
  from HourStatistics where DateHour >= utc_timestamp() - interval $d day group by 1";

$conn = get_mysql_connection();
$conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
$result = $conn->query($sql);
$conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

$data = array();

while($row = $result->fetch_object()) {
  $d = array();
  $d["date_hour"] = $row->DateHour;
  $d["tweets"] = intval($row->Tweets);
  $data[] = $d;
}

$conn->close();

echo json_encode($data);
?>