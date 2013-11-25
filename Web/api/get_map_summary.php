<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
    include 'database.php';
    $conn = get_mysql_connection();

    $sql =
"select 
    PlaceName,
    Latitude,
    Longitude,
    count(distinct TweetID) as TweetCount,
    (
        select 
            concat(ucase(p.PlaceName),' [',ShortDate(t.CreatedAt),']\n', t.Text)
        from 
            WordTweet wt
            join Tweet t on t.TweetID=wt.TweetID
            left join Tweet t2 on t2.RetweetOf=t.TweetID
        where wt.WordID=w.WordID and t.Novelty>0.2
        group by t.TweetID
        order by 
            utc_timestamp() - interval 24 hour < t.CreatedAt desc, 
            count(t2.TweetID) desc, 
            t.CreatedAt desc
        limit 1
    ) as Summary
from 
    (
        select PlaceNameID, PlaceName, Longitude, Latitude, NaiveStemming(PlaceName) as WordStem
        from PlaceName
    ) p
    join Word w on w.Word=p.WordStem
    join WordTweet wt on wt.WordID=w.WordID
group by p.PlaceNameID";

    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
    $result = $conn->query($sql);
    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

    while($row = $result->fetch_object()) {
        $tweet = array();
        $tweet["place_name"] = $row->PlaceName;
        $tweet["latitude"] = $row->Latitude;
        $tweet["longitude"] = $row->Longitude;
        $tweet["tweet_count"] = $row->TweetCount;
        $tweet["summary"] = $row->Summary;
        $tweets[] = $tweet;
    }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($tweets);
?>