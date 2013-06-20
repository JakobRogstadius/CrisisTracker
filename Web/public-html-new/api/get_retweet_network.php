<?php
header('Access-Control-Allow-Origin: *');

$id = intval($_REQUEST['id']);

include 'database.php';

$sql =
    "select t.TweetID, 
        if(firstT.TweetID=t.TweetID, null, coalesce(RetweetOf, firstT.TweetID)) Target,
        RetweetOf,
        CreatedAt, tu.UserID, ScreenName, RealName, if(firstT.TweetID=t.TweetID, Text, null) Text
    from Tweet t
    join TweetCluster tc on tc.TweetClusterID=t.TweetClusterID
    join Story s on s.StoryID=tc.StoryID
    join TwitterUser tu on tu.UserID=t.UserID
    left join
        (select TextHash, min(TweetID) TweetID from (
        select TextHash, TweetID
        from Tweet t
        join TweetCluster tc on tc.TweetClusterID=t.TweetClusterID
        where tc.StoryID=$id
        order by TweetID limit 1000
        ) x
        group by TextHash) firstT on firstT.TextHash=t.TextHash
    where s.StoryID=$id
    order by TweetID
    limit 1000";

$nodes = array();
$links = array();
$map = array();

$conn = get_mysql_connection();

$result = $conn->query($sql);
while($row = $result->fetch_object()) {
  $map[$row->TweetID] = true;
  
  $node = array();
  $node["tweet_id"] = $row->TweetID;
  $node["is_first"] = is_null($row->Text) ? false : true;
  $node["is_retweet"] = is_null($row->RetweetOf) ? false : true;
  $node["user_id"] = $row->UserID;
  $node["user_real_name"] = $row->RealName;
  $node["user_screen_name"] = $row->ScreenName;
  $node["created_at"] = $row->CreatedAt;
  $node["text"] = $row->Text;
  $nodes[] = $node;
  
  if (array_key_exists($row->Target, $map)) {
      $link = array();
      $link["source"] = $row->TweetID;
      $link["target"] = $row->Target;
      $links[] = $link;
  }
}

$conn->close();

$data = array();
$data["nodes"] = $nodes;
$data["links"] = $links;

echo json_encode($data);
?>