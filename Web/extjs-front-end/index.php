<?php
/*******************************************************************************
 * Copyright (c) 2012-2013 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/
$debugflag = true;

if ($debugflag)
{
    ini_set('display_errors', 1); 
    ini_set('log_errors', 1); 
    ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt'); 
    error_reporting(E_ALL);
}

header( 'Content-Type: text/html; charset=UTF-8' );
mb_internal_encoding( 'UTF-8' );

include_once ('login/login.php');
//TODO: Set some information that allows the login page to redirect back to where the user came from

?>
<!DOCTYPE html>
<html lang="en">
    <head>
        <title>CrisisTracker</title>
		
        <meta charset="utf-8" />
        <meta name="description" content="Crisis Tracker lets you explore Twitter activity related to ongoing real-world events." />
        <meta name="keywords" content="crisis, emergency, disaster, protest, conflict, social media, twitter, crowdsourcing, crisis informatics" />
        
        <? if ($debugflag) echo "<script>var debugflag=true;</script>"; ?>
        
        <!-- General page layout (CSS) -->
        <link rel="stylesheet" type="text/css" href="main.css" />

        <!-- Google Analytics -->
        <script src="google_analytics.js"></script>

        <!-- Get Satisfaction tab -->
        <script src="get_satisfaction.js"></script>
		
        <!-- Open Layers -->
        <link rel="stylesheet" type="text/css" href="libraries/full/OpenLayers-2.12/theme/default/style.css" />
<? if($debugflag) { ?>
        <script src="libraries/full/OpenLayers-2.12/OpenLayers.debug.js"></script>
<? } else { //TODO: UPDATE PATH ?>
       <script src="libraries/compressed/OpenLayers-2.12/OpenLayers.debug.js"></script>
<? } ?>
        <!-- Google Maps API -->
		<script src="http://maps.google.com/maps/api/js?sensor=false&v=3.2"></script>		
		
		<!-- ExtJs: Ext and GeoExt files are fetched by the loader. See in app.js -->
        <link rel="stylesheet" type="text/css" href="libraries/full/ext-4/resources/css/ext-all.css">
<? if($debugflag) { ?>
		<script src="libraries/full/ext-4/ext-all-dev.js"></script>	
<? } else { //TODO: UPDATE PATH ?>
		<script src="libraries/compressed/ext-4/ext-all-dev.js"></script>	
<? } ?>
        <script src="app.js"></script>
    </head>
    
    <body>
        <div id="container">
            <div id="header">
                <header>
                    <div style="position: relative">
                        <div id="top-title"><h1>CRISIS TRACKER</h1></div>
                        <div id="login-box" style="z-index: 100; position: absolute; right:4px; bottom: 8px;"><?php //printLogin(); ?></div>
                    </div>
                    <div id="menu"><nav>
                        <ul>
                            <!-- TODO: Add menu highlights -->
                            <li><a href="explorestories.php">Read stories</a></li>
                            <li><a href="tagstorylist.php">Tag stories</a></li>
                            <li><a href="performance.php">Performance</a></li>
                            <li><a href="about.php">About</a></li>
                            <li><a href="evaluation.php">Evaluation</a></li>
                        </ul>
                    </nav></div>
                </header>
            </div>
			
			<!-- ExtJS Draws in Here -->
            <section id="content">			
			</section>
			
            <footer id="footer">
                All timestamps are in UTC.
                <span style="float: right">CrisisTracker is free and <a href="http://github.com/JakobRogstadius/CrisisTracker" target="_blank">open source</a> (<a href="http://www.eclipse.org/legal/epl-v10.html" target="_blank">license</a>)</span>
            </footer>
        </div>        
    </body>
</html>

  