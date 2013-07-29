<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

ini_set('display_errors', 1);
ini_set('log_errors', 1);
ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt');
error_reporting(E_ALL);

session_start();

include_once ('api/common_functions.php');
include_once ('twitteroauth/login.php');
set_login_redirect($_SERVER['REQUEST_URI'], "$SITEPATH/"); //from login.php

header( 'Content-Type: text/html; charset=UTF-8' );
mb_internal_encoding( 'UTF-8' );
function addLinks($text) {
  $text = preg_replace('/http\S+/', '<a href="$0" target="_blank">$0</a>', $text);
  return preg_replace('/@[A-Za-z0-9_]+/', '<a href="https://twitter.com/#!/$0" target="_blank">$0</a>', $text);
}
?>

<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title><?php echo (isset($documentTitle) ? $documentTitle : "CrisisTracker"); ?></title>
    <meta name="description" content="Crisis Tracker lets you explore Twitter activity related to ongoing real-world events." />
    <meta name="keywords" content="crisis, emergency, disaster, protest, conflict, social media, twitter, crowdsourcing, crisis informatics" />

    <!-- *** STATS AND FEEDBACK ************************************************************************* -->
    <?php include('includes.html'); ?>

    <!-- *** CSS *************************************************************************************** -->
    <!-- General page layout -->
    <link rel="stylesheet" href="resources/css/main.css" type="text/css" media="screen" />

    <!-- *** LIBRARIES ********************************************************************************* -->
    <!-- jQUERY -->
    <link rel="stylesheet" type="text/css" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/themes/start/jquery-ui.css" />
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.7.1.min.js"></script>
    <script type='text/javascript' src='https://ajax.googleapis.com/ajax/libs/jqueryui/1.8.13/jquery-ui.min.js'></script>
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery.validate/1.7/jquery.validate.min.js"></script>
    <script type="text/javascript" src="resources/javascript/jquery.cookie.js"></script>

    <!-- *** CUSTOM SCRIPTS **************************************************************************** -->
    <script type="text/javascript">
      //IE has no console before it's activated. Eat all console calls.
      if (typeof console === "undefined"){
        console={};
        console.log = function() {
          return;
        }
      }
      //ByPass New Openlayers.js console window firebug -->
      console.firebug=true;

      function GotoStory(storyID, e) {
        if (e.ctrlKey || e.shiftKey)
          window.open("story.php?storyid=" + storyID, "_blank");
        else
          window.open("story.php?storyid=" + storyID, "_self");
      }
    </script>
    <!-- Alert users with unsupported browsers that CrisisTracker won't work -->
    <script type="text/javascript" src="resources/javascript/browsercheck.js"></script>

    <!-- XML Parser * Cross Domain - Story <-> Database communication -->
    <script type="text/javascript" src="database/xml_parser.js"></script>

<?php
if (false) {//(strpos($_SERVER['REQUEST_URI'], 'study') <= 0) {
?>
    <!-- REMOVE -->
    <!-- LoginFancyBox -->
    <link rel="stylesheet" type="text/css" href="login/fancybox/jquery.fancybox-1.3.4.css" media="screen" />
    <script type="text/javascript" src="login/fancybox/jquery.mousewheel-3.0.4.pack.js"></script>
    <script type="text/javascript" src="login/fancybox/jquery.fancybox-1.3.4.pack.js"></script>
    <!-- Random Generators -->
    <script type="text/javascript" src="resources/javascript/ses.randomgenerators.js"></script>
    <!-- Login Component -->
    <script type="text/javascript" src="login/login.js"></script>
<?php
}
?>
    <!-- *** PAGE SPECIFIC ***************************************************************************** -->
