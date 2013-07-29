<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

function parseTopicFilter($name) {
  if (key_exists($name, $_REQUEST)) {
    $filters = array();

    $filterStr = $_REQUEST[$name];
    $attributeStrings = explode(";", $filterStr);
    for ($i=0; $i<sizeof($attributeStrings); $i++) {
      $tmp = explode(":", $attributeStrings[$i]);
      $f = new stdClass;
      $f->attributeID = intval($tmp[0]);
      $f->labelIDs = array_map("intval", explode(",", $tmp[1]));
      $filters[] = $f;
    }
    return $filters;
  }
  return null;
}

$tweets = array();
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $mintime = getTimeParam("mintime");
  $maxtime = getTimeParam("maxtime");
  $textFilter = getSafeTextParam("text", $conn);
  $fetchOngoingStories = getIntParam("ongoing", 0, 1, 1);
  $topicFilter = parseTopicFilter("topics");

  $where = '';
  $having = '';
  $join = '';
  if (!$fetchOngoingStories && $mintime != null) {
    $where .= " and Story.StartTime >= '$mintime' ";
  }
  if (!$fetchOngoingStories && $maxtime != null) {
    $where .= " and Story.StartTime <= '$maxtime' ";
  }
  if ($textFilter != null) {
    $textFilter = preg_replace("/ *, */", ",", $textFilter);
    $words = stemMany(preg_split("/,| |;/", $textFilter, -1, PREG_SPLIT_NO_EMPTY));
    $textFilter = "'" . implode("','", $words) . "'";

    $where .= " and Keyword in ($textFilter) ";

    $join .=
      " join StoryInfoKeywordTag sikt on sikt.StoryID=Story.StoryID
      join InfoKeyword ik on ik.InfoKeywordID=sikt.InfoKeywordID ";

    if (strlen($having)>0) { $having .= ' and '; }
    $having .= " count(distinct ik.InfoKeywordID)=" . sizeof($words) . " ";
  }

  if ($topicFilter != null) {
    $join .=
      " join StoryAidrAttributeTag saat on saat.StoryID=Story.StoryID and IsMajorityTag=1
      join AidrLabel al on al.LabelID=saat.LabelID ";

    $where .= " and (";
    for ($i=0; $i<sizeof($topicFilter); $i++) {
      if ($i>0) { $where .= ' or '; }
      $where .= "saat.LabelID in (" . implode(",", $topicFilter[$i]->labelIDs) . ") ";
    }
    $where .= ") ";

    if (strlen($having)>0) { $having .= ' and '; }
    $having .= " count(distinct al.AttributeID)=" . sizeof($topicFilter) . " ";
  }
  if (strlen($having)>0) { $having = 'having ' . $having; }


  $orderByInner = ($fetchOngoingStories
              ? "TopUserCountRecent desc, WeightedSizeRecent desc"
                : "TopUserCount desc, StartTime desc");
  //$orderByOuter = ($fetchOngoingStories ? "" : "order by StartTime desc");
  $orderByOuter = "order by StartTime desc";

  $sql =
      "select * from (
        select
          Story.StoryID,
          UserCount,
          round(WeightedSize) WeightedSize,
          StartTime,
          Title
        from Story $join
        where not IsHidden $where
        group by Story.StoryID
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
    $story["user_count"] = $row->UserCount;
    $story["weighted_size"] = $row->WeightedSize;
//    $story["start_time"] = date_format(date_create($row->StartTime), 'Y-m-d\TH:i:s\Z');
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