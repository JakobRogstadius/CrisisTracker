<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include_once ('../api/common_functions.php');

// CACHING SERVER
header("Content-type: text/xml");

$storyid = $_GET["storyid"];
$onlytags = $_GET["onlytags"];

$url = "$SITEURL/api/get_story.php?storyid=$storyid&onlytags=$onlytags";
// echo $url;
// storyid=5&onlytags=1

$xml = file_get_contents($url);
//$xml = file_get_contents('http://ufn.virtues.fi/~jakob/twitter/get_story.php');

echo $xml;

?>