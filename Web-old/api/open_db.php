<?php
$db_conn = mysql_connect("IP/URL", "USERNAME", "PASSWORD") or die("Database error.");
mysql_select_db("DATABASENAME") or die("Database error.");
mysql_query( "SET NAMES utf8", $db_conn );
mysql_set_charset('utf8', $db_conn);
?>