<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include_once ('../api/common_functions.php');

/***************************************************************/
// Cross Domain REDIRECTOR HACK
header("Content-type: text/xml");

//Initialize (8 params)
$sortorder="";
$limit="";
$minstarttime="";
$maxstarttime="";
$locationfilter="";
$categoryfilter="";
$entityfilter="";
$keywordfilter="";


/***************************************************************
* EXTRACT GET
***************************************************************/
// create sufix
$suffix="";

//***************************************************************
// Variables
if (isset($_GET['sortorder'])) {
	$suffix.="sortorder=".$_GET["sortorder"];
}
if (isset($_GET['hidearabic'])) {
	$suffix.="&hidearabic=".$_GET["hidearabic"];
}
if (isset($_GET['limit'])) {
	$suffix.="&limit=".$_GET["limit"];
}
if (isset($_GET['minstarttime'])) {
	$suffix.="&minstarttime=" . urlencode($_GET["minstarttime"]);
}
if (isset($_GET['maxstarttime'])) {
	$suffix.="&maxstarttime=" . urlencode($_GET["maxstarttime"]);
}
if (isset($_GET['locationfilter'])) {
	$suffix.="&locationfilter=".$_GET["locationfilter"];
}

//***************************************************************
// Arrays
if (isset($_GET['categoryfilter'])) {
	foreach($_GET['categoryfilter'] as $k => $keyword){ 
		$suffix.="&categoryfilter[]=".$keyword;
	}	
}

if (isset($_GET['entityfilter'])) {
	foreach($_GET['entityfilter'] as $k => $keyword){ 
		$suffix.="&entityfilter[]=".rawurlencode($keyword);
	}	
}

if (isset($_GET['keywordfilter'])) {
	foreach($_GET['keywordfilter'] as $k => $keyword){ 
		$suffix.="&keywordfilter[]=".rawurlencode($keyword);
	}		
}

//***************************************************************
/* Compose URL

$url = http://ufn.virtues.fi/~jakob/twitter/api/get_stories.php?sortorder=recent&categoryfilter[]=2&categoryfilter[]=5&locationfilter=0,0,100,100&keywordfilter[]=daraa&minstarttime=2012-02-22
*/
//echo $suffix;

$url = "$SITEURL/api/get_stories.php?$suffix";
//echo $url;

$xml = file_get_contents($url);
echo $xml;
?>