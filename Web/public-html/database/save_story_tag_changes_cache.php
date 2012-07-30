<?php
// CACHING SERVER
header("Content-type: text/xml");

//Initialize
$storyid="";
$userid="";
$addedcategories="";
$addedentities="";
$addedkeywords="";
$removedcategories="";
$removedentities="";
$removedkeywords="";
$addedlocationslatitude="";
$addedlocationslongitude="";
$removedlocations="";

// Extract GET
// create sufix
$suffix="";

if (isset($_GET['storyid'])) {
	$storyid = $_GET["storyid"];	
}
if (isset($_GET['userid'])) {
	$userid = $_GET["userid"];
}

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
	 // $arr = unserialize($_GET["arr"]);
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

$url = 'http://ufn.virtues.fi/~jakob/twitter/api/save_story_tag_changes.php?storyid='.$storyid.="&userid=".$userid.$suffix;
// echo $url;

$xml = file_get_contents($url);

echo $xml;


?>