<?php

/*
INPUT:
  categoryfilter[]: int
  entityfilter[]: string
  keywordfilter[]: string
  locationfilter: minLong, minLat, maxLong, maxLat
  minstarttime: YYYY-MM-DD hh:mm:ss
  maxstarttime: YYYY-MM-DD hh:mm:ss
  sortorder, one of:
    Active
    Recent
    Large
  limit: int

Sort orders:
  active:   least(UserCountRecent, TweetCountRecent + 0.5*log(10+RetweetCountRecent))
  recent:   age
  large:    least(UserCount, TweetCount + 0.5*log(10+RetweetCount))

NOTE: locationfilter will break whenever the box overlaps +-360

OUTPUT:
  Array of stories, containing subset of fields:
    StoryID
    Title
    UserCount
    StartTime
    LocationCount
    CategoryCount
    Categories
      ID
      Name
    EntityCount

*/

$LARGEST = 'largest';
$TOP_USERS = 'top';
$RECENT = 'recent';
$ACTIVE = 'active';
$TIMELINE = 'timeline';

function getSafeValues($input, $readNumbers = FALSE) {
  $output = array();
  if (is_array($input)) {
    foreach($input as $tag) {
      $tmp = '';
      if ($readNumbers)
        $tmp = intval($tag);
      else
        $tmp = stripMaliciousSql($tag);
      if ($tmp != '')
        $output[] = $tmp;
    }
  }
  return $output;
}

function naiveStemming($words) {
  for ($i=0; $i<count($words); $i++) {
    if (strlen($words[$i]) < 4 || substr_compare($words[$i],"#",0,1) == 0)
      return str;
    $before = $words[$i];
    while (($words[$i] = preg_replace("/(es|ed|s|ing|ly|n)$/", "", $words[$i])) != $before)
      $before = $words[$i];
  }
  return $words;
}

ini_set('display_errors', 1);
ini_set('log_errors', 1);
ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt');
error_reporting(E_ALL);

header( 'Content-Type: text/xml; charset=UTF-8' );
mb_internal_encoding( 'UTF-8' );

include('common_functions.php');
include('open_db.php');

//ORDER BY
$sortOrder = $LARGEST;
$topusers = false;
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if (substr($sortOrder, -strlen($TOP_USERS)) == $TOP_USERS)
    $topusers = true;
  $sortOrder = substr($sortOrder, 0, -4); //remove -top/-all ending
}
$orderby = '';
if ($sortOrder == $LARGEST || $sortOrder == $TIMELINE)
  $orderby = ($topusers ? 'TopUserCount' : 'WeightedSize');
elseif ($sortOrder == $ACTIVE)
  $orderby = ($topusers ? 'TopUserCountRecent' : 'WeightedSizeRecent');
else //recent
  $orderby = 'StartTime';

//echo "Top users: " . ($topusers ? "true" : "false") . "\n";
//echo "Sort order: " . $sortOrder . "\n";

//LIMIT
$limit = 0;
if (isset($_GET['limit'])) {
  $limit = intval($_GET['limit']);
}
if ($limit < 1 || $limit > 200)
  $limit = 20;

//WHERE
$where = "where not IsHidden ";
$wherejoins = "";
$hitcounts = "";
$having = "having ";

if ($topusers) {
    $where .= "and TopUserCount>0";
}

//Min StartTime filter
if (isset($_GET['minstarttime'])) {
  $minTime = stripMaliciousSql($_GET['minstarttime']);
  if (strtotime($minTime) != FALSE) {
    if ($where != 'where ') $where .= ' and ';

    $where .= " StartTime >= '" . $minTime ."'";
  }
}

//Max StartTime filter
if (isset($_GET['maxstarttime'])) {
  $maxTime = stripMaliciousSql($_GET['maxstarttime']);
  if (strtotime($maxTime) != FALSE) {
    if ($where != 'where ') $where .= ' and ';

    $where .= " StartTime <= '" . $maxTime ."'";
  }
}

//Category filter
if (isset($_GET['categoryfilter'])) {
  $categoryIDs = getSafeValues($_GET['categoryfilter'], TRUE);
  if (sizeof($categoryIDs) > 0) {
    if ($where != 'where ') $where .= ' and ';
    if ($having != 'having ') $having .= ' and ';

    $where .= " InfoCategoryID in (" . implode(',', $categoryIDs) . ')';
    $wherejoins .= "join StoryInfoCategoryTag cat on cat.StoryID=Story.StoryID ";
    $hitcounts .= "count(distinct cat.InfoCategoryID) CategoryHitCount,";
    $having .= "CategoryHitCount=" . sizeof($categoryIDs);
  }
}

//Keyword filter
if (isset($_GET['keywordfilter'])) {
  $keywords = naiveStemming(getSafeValues($_GET['keywordfilter']));
  if (sizeof($keywords) > 0) {
    if ($where != 'where ') $where .= ' and ';
    if ($having != 'having ') $having .= ' and ';

    $where .= " InfoKeywordID in (select InfoKeywordID from InfoKeyword where Keyword in ('" . strtolower(implode("','", $keywords)) . "'))";
    $wherejoins .= "join StoryInfoKeywordTag keyword on keyword.StoryID=Story.StoryID ";
    $hitcounts .= "count(distinct keyword.InfoKeywordID) KeywordHitCount,";
    $having .= "KeywordHitCount=" . sizeof($keywords);
  }
}

//Entity filter
if (isset($_GET['entityfilter'])) {
  $entities = getSafeValues($_GET['entityfilter']);
  if (sizeof($entities) > 0) {
    if ($where != 'where ') $where .= ' and ';
    if ($having != 'having ') $having .= ' and ';

    $where .= " InfoEntityID in (select InfoEntityID from InfoEntity where Entity in ('" . strtolower(implode("','", $entities)) . "'))";
    $wherejoins .= "join StoryInfoEntityTag entity on entity.StoryID=Story.StoryID ";
    $hitcounts .= "count(distinct entity.InfoEntityID) EntityHitCount,";
    $having .= "EntityHitCount=" . sizeof($entities);
  }
}

//Location filter
if (isset($_GET['locationfilter'])) {
  $locations = stripMaliciousSql($_GET['locationfilter']);
  if (strlen($locations) > 0) {
    $bounds = explode(',', $locations, 4);
    $minLon = floatval($bounds[0]);
    $minLat = floatval($bounds[1]);
    $maxLon = floatval($bounds[2]);
    $maxLat = floatval($bounds[3]);

    if ($where != 'where ') $where .= ' and ';

    $where .= " Latitude between $minLat and $maxLat and Longitude between $minLon and $maxLon";
    $wherejoins .= "join StoryLocationTag loc on loc.StoryID=Story.StoryID ";
  }
}

if ($where == 'where ') $where = '';
if ($having == 'having ') $having = '';

$orderbyOuter = "sortorder";
if ($sortOrder == 'timeline' || $sortOrder == 'active') {
  $orderbyOuter = "StartTimeRaw";
}

$sqlQuery = "select
    T.*,
    count(distinct loc.TagID) LocationCount,
    count(distinct cat.InfoCategoryID) CategoryCount,
    group_concat(distinct InfoCategoryID order by InfoCategoryID separator ',') as Categories,
    count(distinct ent.InfoEntityID) EntityCount,
    group_concat(distinct concat(Longitude, ',', Latitude) separator ';') as GeoTags
from (
        select
            Story.StoryID,
            coalesce(Title,'null') Title,
            " . ($topusers ? 'TopUserCount' : 'round(WeightedSize)') . " as Size,
            ShortDate(StartTime) as StartTime,
            StartTime as StartTimeRaw,
            $hitcounts
            $orderby as sortorder
        from Story
            $wherejoins
        $where
        group by Story.StoryID
        $having
        order by sortorder desc
        limit $limit
    ) T
    left join StoryLocationTag loc on loc.StoryID=T.StoryID
    left join StoryInfoEntityTag ent on ent.StoryID=T.StoryID
    left join StoryInfoCategoryTag cat on cat.StoryID=T.StoryID
    left join StoryInfoKeywordTag keyword on keyword.StoryID=T.StoryID
group by T.StoryID
order by $orderbyOuter desc";

//echo $sqlQuery;
// Story info
$storyInfoResult = mysql_query($sqlQuery, $db_conn);

$stories = array();
while($row = mysql_fetch_array($storyInfoResult)) {
  $story = array();
  $story['storyID'] = $row['StoryID'];
  $story['title'] = htmlspecialchars($row['Title']);
  $story['userCount'] = $row['Size'];
  $story['startTime'] = $row['StartTime'];
  $story['locationCount'] = $row['LocationCount'];
  $story['entityCount'] = $row['EntityCount'];
  $story['categoryCount'] = $row['CategoryCount'];

  if (strlen($row['Categories']) > 0) {
    $story['categories'] = array();
    $categoryIDTags = explode(',', $row['Categories']);
    foreach($categoryIDTags as $id) {
      $tag = array();
      $tag['id'] = $id;
      $story['categories'][] = $tag;
    }
  }

  if (strlen($row['GeoTags']) > 0) {
    $story['locations'] = array();
    $geotags = explode(';', $row['GeoTags']);
    foreach($geotags as $gt) {
      $longlat = explode(',', $gt);
      $tag = array();
      $tag['longitude'] = $longlat[0];
      $tag['latitude'] = $longlat[1];
      $story['locations'][] = $tag;
    }
  }

  $stories[] = $story;
}

$xmlResult = array();
$xmlResult['storiesCount'] = sizeof($stories);
$xmlResult['stories'] = $stories;
//Print
echo array_to_xml($xmlResult, new SimpleXMLElement('<xmlResult/>'))->asXML();

include('close_db.php');
?>