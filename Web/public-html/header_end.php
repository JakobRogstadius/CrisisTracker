<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/
 
$menuHighlight = '';
if (strpos($_SERVER['REQUEST_URI'], 'explorestories') > 0)
  $menuHighlight = 'read';
elseif (strpos($_SERVER['REQUEST_URI'], 'tagstorylist') > 0)
  $menuHighlight = 'tag';  
elseif (strpos($_SERVER['REQUEST_URI'], 'performance') > 0)
  $menuHighlight = 'performance';  
elseif (strpos($_SERVER['REQUEST_URI'], 'about') > 0)
  $menuHighlight = 'about';
elseif (strpos($_SERVER['REQUEST_URI'], 'evaluation') > 0)
  $menuHighlight = 'evaluation';
?>

  <script>
    function toggleArabic() {
      $.cookie('hidearabic', $("#chk-hide-arabic").attr('checked') == 'checked');
    }
    //if ($.cookie("hidearabic") == "true")
    //  $("#chk-hide-arabic").attr('checked','checked');
  </script>
  </head>
  <body>
    <div id="container">
      <div id="header">
        <header>
          <div style="position: relative">
            <div id="top-title"><h1>CRISIS TRACKER</h1></div>
            <div id="login-box" style="z-index: 100; position: absolute; right:4px; bottom: 8px;"><input type="checkbox" id="chk-hide-arabic" <?php if(isset($_COOKIE["hidearabic"]) && $_COOKIE["hidearabic"] == "true") echo 'checked="checked"'; ?> onchange="toggleArabic()" /><label for="chk-hide-arabic">Hide content in Arabic</label> | <?php printLogin(); ?></div>
          </div>
          <div id="menu"><nav>
            <ul>
              <li <?php if ($menuHighlight=='read')   echo 'class="active"'; ?>><a href="explorestories.php">Read stories</a></li>
              <li <?php if ($menuHighlight=='tag')    echo 'class="active"'; ?>><a href="tagstorylist.php">Tag stories</a></li>
              <li <?php if ($menuHighlight=='performance')    echo 'class="active"'; ?>><a href="performance.php">Performance</a></li>
              <li <?php if ($menuHighlight=='about')  echo 'class="active"'; ?>><a href="about.php">About</a></li>
              <li <?php if ($menuHighlight=='evaluation')  echo 'class="active"'; ?>><a href="evaluation.php">Evaluation</a></li>
            </ul>
          </nav></div>
        </header>
      </div>
      <section id="content">
<?php
/* VOLUNTEER SURVEY
include_once('api/open_db.php');
include_once('twitterLogin/login.php');
$userID=getUserID();
if (is_null($userID))
  $userID=0;
$userAnsweredResult = mysql_query("select count(*) as 'answers' from SyriaDeploymentVolunteerSurveyAnswer where TwitterUserID=$userID;");
$resultObj = mysql_fetch_object($userAnsweredResult);
$answeredVolunteerSurvey = ($resultObj->answers > 0);
//include('api/close_db.php');
if (!$answeredVolunteerSurvey && strpos($_SERVER['REQUEST_URI'], 'survey-volunteer') <= 0) {
?>
        <div id="survey-ad" class="fullwidth-column">
          <div class="gui-panel" style="font-weight: bold; font-size: 130%; color: #393; border: 2px solid #393; background-color: white;">
            Please help conclude the evaluation by filling in <a href="survey-volunteer.php">a quick survey</a>.
          </div>
        </div>
<?php
}
*/
?>