<?php
$menuHighlight = '';
if (strpos($_SERVER['REQUEST_URI'], 'explorestories') > 0)
  $menuHighlight = 'read';
elseif (strpos($_SERVER['REQUEST_URI'], 'tagstorylist') > 0)
  $menuHighlight = 'tag';  
elseif (strpos($_SERVER['REQUEST_URI'], 'performance') > 0)
  $menuHighlight = 'performance';  
elseif (strpos($_SERVER['REQUEST_URI'], 'about') > 0)
  $menuHighlight = 'about';
elseif (strpos($_SERVER['REQUEST_URI'], 'study') > 0)
  $menuHighlight = 'study';
?>
  </head>
  <body>
    <div id="container">
      <div id="header">
	<header>
	  <div id="top-title"><h1>CRISIS TRACKER</h1></div>
	  <div id="menu"><nav>
	    <ul>
	      <li <?php if ($menuHighlight=='read')   echo 'class="active"'; ?>><a href="explorestories.php">Read stories</a></li>
	      <li <?php if ($menuHighlight=='tag')    echo 'class="active"'; ?>><a href="tagstorylist.php">Tag stories</a></li>
	      <li <?php if ($menuHighlight=='performance')    echo 'class="active"'; ?>><a href="performance.php">Performance</a></li>
	      <li <?php if ($menuHighlight=='about')  echo 'class="active"'; ?>><a href="about.php">About</a></li>
	      <li <?php if ($menuHighlight=='study')  echo 'class="active"'; ?>><a href="study.php">Study</a></li>
	    </ul>
	  </nav></div>
	</header>
      </div>
      <section id="content">
