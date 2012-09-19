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
include('../twitterLogin/login.php');

if (!isLoggedIn() && !isset($_GET['userid'])) {
  exit("missing user id");
}

//Validate inputs
if (!isset($_GET['storyid']))
  exit("missing storyid");

$storyID = intval($_GET['storyid']);
$ip = $_SERVER['REMOTE_ADDR'];
if (isset($_GET['userip'])) {
  $ip = stripMaliciousSql($_GET['userip']);
}
$userID = getUserID();
if (isset($_GET['userid'])) {
  $userID = intval($_GET['userid']);
}

$outputData = array();

include('open_db.php');

//Delete removed locations
if (isset($_GET['removedlocations'])) {
  $removedLocationIDs = getSafeValues($_GET['removedlocations'], TRUE);
  foreach ($removedLocationIDs as $tagID) {
    mysql_query("call AddRemoveStoryTag(0, 'location', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
  }
}

//Delete removed categories
if (isset($_GET['removedcategories'])) {
  $removedCategoryIDs = getSafeValues($_GET['removedcategories'], TRUE);
  foreach ($removedCategoryIDs as $tagID) {
    mysql_query("call AddRemoveStoryTag(0, 'category', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
  }
}

//Delete removed entities
if (isset($_GET['removedentities'])) {
  $removedEntityIDs = getSafeValues($_GET['removedentities'], TRUE);
  foreach ($removedEntityIDs as $tagID) {
    mysql_query("call AddRemoveStoryTag(0, 'entity', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
  }
}

//Delete removed keywords
if (isset($_GET['removedkeywords'])) {
  $removedKeywordIDs = getSafeValues($_GET['removedkeywords'], TRUE);
  foreach ($removedKeywordIDs as $tagID) {
    mysql_query("call AddRemoveStoryTag(0, 'keyword', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
  }
}


//Insert added categories
if (isset($_GET['addedcategories']))
{
  $addedCategoryIDs = getSafeValues($_GET['addedcategories'], TRUE);
  foreach ($addedCategoryIDs as $tagID) {
    mysql_query("call AddRemoveStoryTag(1, 'category', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
  }
}

//Insert added entities
if (isset($_GET['addedentities']))
{
  $addedEntityNames = getSafeValues($_GET['addedentities']);
  $addedEntityNames = array_diff($addedEntityNames, array(''));

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
    
    foreach (array_keys($entityTags) as $tagID) {
      mysql_query("call AddRemoveStoryTag(1, 'entity', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
    }
  }
}

//Insert added keywords
if (isset($_GET['addedkeywords']))
{
  $addedKeywordNames = naiveStemming(getSafeValues($_GET['addedkeywords']));
  $addedKeywordNames = array_diff($addedKeywordNames, array(''));
  
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
    
    foreach (array_keys($keywordTags) as $tagID) {
      mysql_query("call AddRemoveStoryTag(1, 'keyword', $userID, INET_ATON('$ip'), $storyID, $tagID, null, null);", $db_conn);
    }
  }
}

//Insert added locations
if (isset($_GET['addedlocationslatitude']) && isset($_GET['addedlocationslongitude']))
{
  $addedLongitudes = getSafeValues($_GET['addedlocationslongitude']);
  $addedLatitudes  = getSafeValues($_GET['addedlocationslatitude']);
  if (count($addedLatitudes) != count($addedLongitudes))
    exit("Lengths of latitude and longiude does not match.");

  if (count($addedLatitudes) > 0) {
    //Insert locations
    for ($i=0; $i<count($addedLatitudes); $i++)
    {
      $addedLatitudes[$i] = round($addedLatitudes[$i], 6);
      $addedLongitudes[$i] = round($addedLongitudes[$i], 6);
      
      mysql_query("call AddRemoveStoryTag(1, 'location', $userID, INET_ATON('$ip'), $storyID, null, " . doubleval($addedLongitudes[$i]) . ',' . doubleval($addedLatitudes[$i]) . ');', $db_conn);
    }
    
    //Get IDs
    $locationIDsResult = mysql_query(
      "select TagID, Longitude, Latitude from StoryLocationTag where StoryID=$storyID and Longitude in ("
      . implode(',', $addedLongitudes)
      . ") and Latitude in ("
      . implode(',', $addedLatitudes)
      . ")"
    );
    
    $outputData['locations'] = array();
    while($row = mysql_fetch_array($locationIDsResult)) {
      $outputData['locations'][] = array('id' => $row['TagID'], 'longitude' => $row['Longitude'], 'latitude' => $row['Latitude']);
    }
  }
}

//Print
echo array_to_xml($outputData, new SimpleXMLElement('<tags/>'))->asXML();

include('close_db.php');
?>