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
  storyid: int
  onlytags: bool

OUTPUT:
  Story: <see documentation>
*/

ini_set('display_errors', 1); 
ini_set('log_errors', 1); 
ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt'); 
error_reporting(E_ALL);

header( 'Content-Type: text/xml; charset=UTF-8' );
mb_internal_encoding( 'UTF-8' );

include('common_functions.php');
include('open_db.php');
include('get_story_content.php');

$storyID = 0;
if (isset($_GET['storyid'])) {
  $storyID = intval($_GET['storyid']);
}
$onlyTags = FALSE;
if (isset($_GET['onlytags'])) {
  $onlyTags = (intval($_GET['onlytags']) != 0);
}
$sortOrder = 'size';
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if ($sortOrder != 'time')
    $sortOrder = 'size';
}

//Define a story object to populate
$story = array('storyID' => $storyID);

if (!$onlyTags)
{
  $story = get_story_content($storyID, $sortOrder, $db_conn); //This replaces the story array object
}
  
// Category tags
$categoryTagsResult = mysql_query(
"select Category, InfoCategoryID
from StoryInfoCategoryTag natural join InfoCategory
where StoryID = $storyID", $db_conn);

$story['categoryCount'] = mysql_num_rows($categoryTagsResult);
if (mysql_num_rows($categoryTagsResult) > 0) {
  $story['categories'] = array();
  
  while($row = mysql_fetch_array($categoryTagsResult)) {
    $tag = array();
    $tag['id'] = $row['InfoCategoryID'];
    $tag['name'] = htmlspecialchars($row['Category']);

    $story['categories'][] = $tag;
  }
}


// Entity tags
$entityTagsResult = mysql_query(
"select Entity, InfoEntityID
from StoryInfoEntityTag natural join InfoEntity
where StoryID = $storyID", $db_conn);

$story['entityCount'] = mysql_num_rows($entityTagsResult);
if (mysql_num_rows($entityTagsResult) > 0) {
  $story['entities'] = array();
  
  while($row = mysql_fetch_array($entityTagsResult)) {
    $tag = array();
    $tag['id'] = $row['InfoEntityID'];
    $tag['name'] = htmlspecialchars($row['Entity']);

    $story['entities'][] = $tag;
  }
}


// Keyword tags
$keywordTagsResult = mysql_query(
"select Keyword, InfoKeywordID
from StoryInfoKeywordTag natural join InfoKeyword
where StoryID = $storyID order by Keyword", $db_conn);

$story['keywordCount'] = mysql_num_rows($keywordTagsResult);
if (mysql_num_rows($keywordTagsResult) > 0) {
  $story['keywords'] = array();
  
  while($row = mysql_fetch_array($keywordTagsResult)) {
    $tag = array();
    $tag['id'] = $row['InfoKeywordID'];
    $tag['name'] = htmlspecialchars($row['Keyword']);

    $story['keywords'][] = $tag;
  }
}


// Location tags
$locationTagsResult = mysql_query(
"select StoryID, TagID, Longitude, Latitude
from StoryLocationTag
where StoryID = $storyID", $db_conn);

$story['locationCount'] = mysql_num_rows($locationTagsResult);
if (mysql_num_rows($locationTagsResult) > 0) {
  $story['locations'] = array();
  
  while($row = mysql_fetch_array($locationTagsResult)) {
    $tag = array();
    $tag['id'] = $row['TagID'];
    $tag['latitude'] = $row['Latitude'];
    $tag['longitude'] = $row['Longitude'];

    $story['locations'][] = $tag;
  }
}

//Print
$xmlStory = array_to_xml($story, new SimpleXMLElement('<story/>'))->asXML();
echo $xmlStory;


include('close_db.php');
?>