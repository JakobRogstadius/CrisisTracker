<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $id = intval($_REQUEST['id']);

  $sql =
  "select
      s.StoryID as StoryID,
      Title,
      CustomTitle,
      StartTime,
      ceil(WeightedSize) as WeightedSize,
      UserCount,
      Hits
  from (
  select
      StoryID2 as StoryID,
      sum(Weight)/(TagWeight1+TagWeight2) as Hits
  from (
      select
          ActiveStory.StoryID as StoryID1,
          sikt2.StoryID as StoryID2,
          sikt2.WeightedSize as WeightedSize2,
          TagWeight1,
          TagWeight2,
          sikt1.Weight
      from (
              select s.StoryID, sum(Weight) as TagWeight1
              from Story s
              left join StoryInfoKeywordTag t on t.StoryID=s.StoryID
              where s.StoryID = $id
              group by s.StoryID
          ) ActiveStory
          join StoryInfoKeywordTag sikt1 on sikt1.StoryID=ActiveStory.StoryID
          join (
              select StoryID, WeightedSize, InfoKeywordID, TagWeight2
              from
                  StoryInfoKeywordTag t
                  natural join
                  (
                      select
                          Story.StoryID,
                          Story.WeightedSize,
                          (select sum(Weight) from StoryInfoKeywordTag t where t.StoryID=Story.StoryID) as TagWeight2
                      from (select StoryID, StartTime-interval 24 hour t1, EndTime+interval 6 hour t2 from Story where StoryID=$id) T,
                          Story
                          left join StoryMerges psm on psm.StoryID2 = Story.StoryID
                      where StartTime between t1 and t2
                          and Story.StoryID != T.StoryID
                          and psm.StoryID2 is null
                      order by WeightedSize desc
                      limit 3000
                  ) NearTimeStoryIDs
              ) sikt2 on sikt2.InfoKeywordID=sikt1.InfoKeywordID
      ) T
      group by StoryID1, StoryID2
      order by Hits desc
      limit 20
  ) T
  natural join Story s
  left join StoryCustomTitle sct on sct.StoryID=s.StoryID;";

  $conn = get_mysql_connection();
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  $stories = array();

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["weighted_size"] = $row->WeightedSize;
    $story["user_count"] = $row->UserCount;
    $story["start_time"] = $row->StartTime;
    $story["title"] = $row->Title;
    $stories[] = $story;
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();
echo json_encode($stories);
?>