<?php
/*
INPUT:
  storyID: long
  addedLocationsLatitude[]: latitude
  addedLocationsLongitude[]: longitude
  addedCategories[]: int
  addedEntities[]: string
  addedKeywords[]: string
  removedLocations[]: int
  removedCategories[]: int
  remvedEntities[]: int
  removedKeywords[]: int

OUTPUT:
  locations[]
    id
    latitude
    longitude
  entities[]
    id: int
    name: string
  keywords[]
    id: int
    keyword: string
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

//Validate inputs
if (!isset($_GET['storyid']))
  exit("missing storyid");
if (!isset($_GET['userid']))
  exit("missing userid");

$storyID = intval($_GET['storyid']);
$userID = intval($_GET['userid']);
$ip = $_SERVER['REMOTE_ADDR'];

$outputData = array();


//Delete removed locations
if (isset($_GET['removedlocations'])) {
  $removedLocationIDs = getSafeValues($_GET['removedlocations']);
  if (count($removedLocationIDs) > 0) {
    mysql_query(
      "delete from StoryLocationTag where StoryID=$storyID and TagID in ('" . implode("','", $removedLocationIDs) . "')",
      $db_conn);
  }
}

//Delete removed categories
if (isset($_GET['removedcategories'])) {
  $removedCategoryIDs = getSafeValues($_GET['removedcategories']);
  if (count($removedCategoryIDs) > 0) {
    mysql_query(
      "delete from StoryInfoCategoryTag where StoryID=$storyID and InfoCategoryID in ('" . implode("','", $removedCategoryIDs) . "')",
      $db_conn);
  }
}

//Delete removed entities
if (isset($_GET['removedentities'])) {
  $removedEntityIDs = getSafeValues($_GET['removedentities']);
  if (count($removedEntityIDs) > 0) {
    mysql_query(
      "delete from StoryInfoEntityTag where StoryID=$storyID and InfoEntityID in ('" . implode("','", $removedEntityIDs) . "')",
      $db_conn);
  }
}

//Delete removed keywords
if (isset($_GET['removedkeywords'])) {
  $removedKeywordIDs = getSafeValues($_GET['removedkeywords']);
  if (count($removedKeywordIDs) > 0) {
    mysql_query(
      "delete from StoryInfoKeywordTag where StoryID=$storyID and InfoKeywordID in ('" . implode("','", $removedKeywordIDs) . "')",
      $db_conn);
  }
}


//Insert added categories
if (isset($_GET['addedcategories']))
{
  $addedCategoryIDs = getSafeValues($_GET['addedcategories'], TRUE);

  if (count($addedCategoryIDs) > 0) {
    mysql_query(
      "insert ignore into StoryInfoCategoryTag (StoryID, UserID, CreatedAt, IP, InfoCategoryID) values ($storyID, $userID, utc_timestamp(), INET_ATON('$ip'), "
      . implode("), ($storyID, $userID, utc_timestamp(), INET_ATON('$ip'), ", $addedCategoryIDs)
      . ")",
      $db_conn);
  }
}

//Insert added entities
if (isset($_GET['addedentities']))
{
  $addedEntityNames = getSafeValues($_GET['addedentities']);

  if (count($addedEntityNames) > 0) {
    //Insert new values
    mysql_query(
      "insert ignore into InfoEntity (Entity) values ('" . implode("'),('", $addedEntityNames) . "')",
      $db_conn);
    
    //Get IDs for inserted values
    $addedEntitiesResult = mysql_query(
      "select InfoEntityID, Entity from InfoEntity where Entity in ('" . implode("','", $addedEntityNames) . "')",
      $db_conn);
    $entityTags = array();
    $outputData['entities'] = array();
    while($row = mysql_fetch_array($addedEntitiesResult)) {
      $entityTags[$row['InfoEntityID']] = $row['Entity'];
      $outputData['entities'][] = array(
        'id' => $row['InfoEntityID'],
        'name' => $row['Entity']);
    }
    
    $entityTagQuery = "insert ignore into StoryInfoEntityTag (StoryID, UserID, CreatedAt, IP, InfoEntityID) values ($storyID, $userID, utc_timestamp(), INET_ATON('$ip'), "
      . implode("), ($storyID, $userID, utc_timestamp(), INET_ATON('$ip'), ", array_keys($entityTags))
      . ")";
   mysql_query($entityTagQuery, $db_conn);
  }
}

//Insert added keywords
if (isset($_GET['addedkeywords']))
{
  $addedKeywordNames = naiveStemming(getSafeValues($_GET['addedkeywords']));

  if (count($addedKeywordNames) > 0) {
    //Insert new values
    mysql_query(
      "insert ignore into InfoKeyword (Keyword) values ('" . implode("'),('", $addedKeywordNames) . "')",
      $db_conn);
    
    //Get IDs for inserted values
    $addedKeywordResult = mysql_query(
      "select InfoKeywordID, Keyword from InfoKeyword where Keyword in ('" . implode("','", $addedKeywordNames) . "')",
      $db_conn);
    $keywordTags = array();
    $outputData['keywords'] = array();
    while($row = mysql_fetch_array($addedKeywordResult)) {
      $keywordTags[$row['InfoKeywordID']] = $row['Keyword'];
      $outputData['keywords'][] = array(
        'id' => $row['InfoKeywordID'],
        'name' => $row['Keyword']);
    }
        
    mysql_query(
      "insert ignore into StoryInfoKeywordTag (StoryID, UserID, CreatedAt, InfoKeywordID) values ($storyID, $userID, utc_timestamp(), "
      . implode("), ($storyID, $userID, utc_timestamp(), ", array_keys($keywordTags))
      . ")",
      $db_conn);
  }
}

//Insert added locations
if (isset($_GET['addedlocationslatitude']) && isset($_GET['addedlocationslongitude']))
{
  $addedLatitudes  = getSafeValues($_GET['addedlocationslatitude']);
  $addedLongitudes = getSafeValues($_GET['addedlocationslongitude']);
  if (count($addedLatitudes) != count($addedLongitudes))
    exit("Lengths of latitude and longiude does not match.");

  if (count($addedLatitudes) > 0) {
    //Insert locations
    $locationSql = "insert ignore into StoryLocationTag (StoryID, UserID, CreatedAt, IP, Latitude, Longitude) values ";
    for ($i=0; $i<count($addedLatitudes); $i++)
    {
      $addedLatitudes[$i] = round($addedLatitudes[$i], 6);
      $addedLongitudes[$i] = round($addedLongitudes[$i], 6);
      if ($i > 0)
        $locationSql .= ',';
      $locationSql .= "($storyID, $userID, utc_timestamp(), INET_ATON('$ip'), " . doubleval($addedLatitudes[$i]) . ',' . doubleval($addedLongitudes[$i]) . ')';
    }
    mysql_query($locationSql, $db_conn);
    
    //Get IDs
    $locationIDsResult = mysql_query(
      "select TagID, Latitude, Longitude from StoryLocationTag where StoryID=$storyID and Latitude in ("
      . implode(',', $addedLatitudes)
      . ") and Longitude in ("
      . implode(',', $addedLongitudes)
      . ")"
    );
    
    $outputData['locations'] = array();
    while($row = mysql_fetch_array($locationIDsResult)) {
      $outputData['locations'][] = array('id' => $row['TagID'], 'latitude' => $row['Latitude'], 'longitude' => $row['Longitude']);
    }
  }
}


//Print
echo array_to_xml($outputData, new SimpleXMLElement('<tags/>'))->asXML();


include('close_db.php');
?>