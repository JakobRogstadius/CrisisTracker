<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
    include 'database.php';
    $conn = get_mysql_connection();

    $textfilter = getSafeTextParam("text", $conn);

    if ($textfilter != null) {
        $where .= " and Text like '%$textfilter%'";
    }

    $sql =
    "select T2.*, RealName, ScreenName, ProfileImageUrl, Story.StoryID, Story.WeightedSize, count(Tweet.TweetID) as Retweets from (
     select * from 
     (select TweetID, UserID, CreatedAt, Text, Novelty, TweetClusterID
     from Tweet where RetweetOf is null and Novelty>0.05 $where order by TweetID desc limit 200) T 
     order by Novelty desc limit 100
     ) T2
     join TwitterUser on TwitterUser.UserID=T2.UserID
     join TweetCluster on TweetCluster.TweetClusterID=T2.TweetClusterID
     join Story on Story.StoryID=TweetCluster.StoryID
     left join Tweet on Tweet.RetweetOf = T2.TweetID
     group by T2.TweetID
     order by Retweets desc, CreatedAt desc";

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
        $tweet["story_id"] = $row->StoryID;
        $tweet["story_weighted_size"] = $row->WeightedSize;
        $tweet["retweet_count"] = $row->Retweets;
        $tweet["text"] = $row->Text;
        $tweets[] = $tweet;
    }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($tweets);
?>