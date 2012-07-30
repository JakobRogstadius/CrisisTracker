<?php
// CACHING SERVER
header("Content-type: text/xml");

$storyid = $_GET["storyid"];
$onlytags = $_GET["onlytags"];

$url = 'http://ufn.virtues.fi/~jakob/twitter/api/get_story.php?storyid='.$storyid."&onlytags=".$onlytags;
// echo $url;
// storyid=5&onlytags=1

$xml = file_get_contents($url);
//$xml = file_get_contents('http://ufn.virtues.fi/~jakob/twitter/get_story.php');

echo $xml;

?>