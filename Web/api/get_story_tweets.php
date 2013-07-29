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
    $topicfilter = getIntParam("label_id", 0, PHP_INT_MAX, null);

    $where = '';
    $join = '';
    if ($textfilter != null) {
        $where .= " and Text like '%$textfilter%'";
    }
    if ($topicfilter != null) {
        $where .= " and LabelID=" . $topicfilter;
        $join .= " join TweetAidrAttributeTag taat on taat.TweetID=t.TweetID";
    }
    $having = '';
    if ($maxtime != null && $mintime != null){
        $having = "having FirstSeen between '$mintime' and '$maxtime'";
    }

    $sql =
    "select X.TweetID, TwitterUser.UserID, RealName, ScreenName, ProfileImageUrl, CreatedAt, UserCount, Text from (
        select
            min(t.TweetID) as TweetID, min(t.CreatedAt) as FirstSeen, count(distinct t.UserID) as UserCount, log(1+count(distinct t.UserID))*pow(max(t.Novelty),2) as OrderByWeight, s.TweetCount
        from Tweet t
            join TweetCluster tc on tc.TweetClusterID=t.TweetClusterID
            join Story s on s.StoryID=tc.StoryID
            join TwitterUser tu on tu.UserID=t.UserID
            $join
        where s.StoryID=$id and not tu.IsBlacklisted $where
        group by t.TextHash
        $having
        order by OrderByWeight desc limit 20
    ) X
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