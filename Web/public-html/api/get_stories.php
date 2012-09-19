<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

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
$sortOrder = 'large';
$topusers = false;
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if (substr($sortOrder, -3) == 'top')
    $topusers = true;
  $sortOrder = substr($sortOrder, 0, -4); //remove -top/-all ending
}
$orderby = '';
if ($sortOrder == 'largest' || $sortOrder == 'timeline')
  $orderby = ($topusers ? 'TopUserCount' : 'Importance');
elseif ($sortOrder == 'active')
  $orderby = ($topusers ? 'TopUserCountRecent' : 'ImportanceRecent');
elseif ($sortOrder == 'trending')
  $orderby = 'Trend';
else //recent
  $orderby = 'StartTime';

//LIMIT
$limit = 0;
if (isset($_GET['limit'])) {
  $limit = intval($_GET['limit']);
}
if ($limit < 1 || $limit > 200)
  $limit = 50;

//WHERE
$where = "where not IsHidden and psm.StoryID2 is null ";
$wherejoins = "";
$hitcounts = "";
$having = "having ";

if ($sortOrder == 'trending' && !$topusers) {
    $where .= "and UserCount>5";
}
else if ($topusers) {
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

//Hide Arabic
if (isset($_GET['hidearabic']) && $_GET['hidearabic'] == "true") {
  $where .= " and Title not regexp '[؀-ۿ]'";
}


if ($where == 'where ') $where = '';
if ($having == 'having ') $having = '';

$orderbyOuter = "sortorder";
if ($sortOrder == 'timeline') {
  $orderbyOuter = "StartTimeRaw";
}
if ($sortOrder == 'active' || $sortOrder == 'trending') {
  $orderbyOuter = "sortorder desc, StartTime";
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
            Title,
            CustomTitle,
            " . ($topusers ? 'TopUserCount' : 'round(exp(Importance))') . " as Size,
            ShortDate(StartTime) as StartTime,
            StartTime as StartTimeRaw,
            $hitcounts
            $orderby as sortorder
        from Story
            left join PendingStoryMerges psm on psm.StoryID2 = Story.StoryID
            left join StoryCustomTitle sct on sct.StoryID=Story.StoryID
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
  if (is_null($row['CustomTitle']))
    $story['title'] = htmlspecialchars($row['Title']);
  else
    $story['title'] = htmlspecialchars(urldecode($row['CustomTitle']));
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