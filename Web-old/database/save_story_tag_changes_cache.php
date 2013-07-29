<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/
include_once ('../api/common_functions.php');
session_start();

// CACHING SERVER
header("Content-type: text/xml");

//Initialize
$storyid="";
$addedcategories="";
$addedentities="";
$addedkeywords="";
$removedcategories="";
$removedentities="";
$removedkeywords="";
$addedlocationslatitude="";
$addedlocationslongitude="";
$removedlocations="";

include('../twitteroauth/login.php');

if (!is_logged_in()) {
  exit("not logged in");
}
$userid = get_user_id();

// Extract GET
// create suffix
$suffix="";

if (isset($_GET['storyid'])) {
	$storyid = $_GET["storyid"];
}
$ip = $_SERVER['REMOTE_ADDR'];

if (isset($_GET['addedcategories'])) {
	foreach($_GET['addedcategories'] as $k => $keyword){
		$suffix.="&addedcategories[]=".$keyword;
	}
}

if (isset($_GET['addedentities'])) {
	foreach($_GET['addedentities'] as $k => $keyword){
		$suffix.="&addedentities[]=".rawurlencode($keyword);
	}
}

if (isset($_GET['addedkeywords'])) {
	foreach($_GET['addedkeywords'] as $k => $keyword){
		$suffix.="&addedkeywords[]=".rawurlencode($keyword);
	}
}

if (isset($_GET['removedcategories'])) {
	foreach($_GET['removedcategories'] as $k => $keyword){
		$suffix.="&removedcategories[]=".$keyword;
	}
}
if (isset($_GET['removedentities'])) {
	foreach($_GET['removedentities'] as $k => $keyword){
		$suffix.="&removedentities[]=".rawurlencode($keyword);
	}
}
if (isset($_GET['removedkeywords'])) {
	foreach($_GET['removedkeywords'] as $k => $keyword){
		$suffix.="&removedkeywords[]=".rawurlencode($keyword);
	}
}

if (isset($_GET['addedlocationslatitude'])) {
	foreach($_GET['addedlocationslatitude'] as $k => $keyword){
		$suffix.="&addedlocationslatitude[]=".$keyword;
	}
}
if (isset($_GET['addedlocationslongitude'])) {
	foreach($_GET['addedlocationslongitude'] as $k => $keyword){
		$suffix.="&addedlocationslongitude[]=".$keyword;
	}
}
if (isset($_GET['removedlocations'])) {
	foreach($_GET['removedlocations'] as $k => $keyword){
		$suffix.="&removedlocations[]=".$keyword;
	}
}

/* Compose URL
$url = 'http://ufn.virtues.fi/~jakob/twitter/save_story_tag_changes.php?storyid='.$storyid.="&userid=".$userid.="&addedcategories[]=".$addedcategories.="&addedentities[]=".$addedentities.="&addedkeywords[]=".$addedkeywords.="&removedcategories[]=".$removedcategories.="&removedentities[]=".$removedentities.="&removedkeywords[]=".$removedkeywords.="&addedlocationslatitude[]=".$addedlocationslatitude.="&addedlocationslongitude[]=".$addedlocationslongitude.="&removedlocations[]=".$removedlocations;
*/
//echo $suffix;

$url = "$SITEURL/api/save_story_tag_changes.php?storyid=$storyid&userip=$ip&userid=$userid" . $suffix;
//echo $url;

$xml = file_get_contents($url);

echo $xml;
?>