<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

$db_conn = mysql_connect("localhost", "USERNAME", "PASSWORD") or die("Database error.");
mysql_select_db("CrisisTracker") or die("Database error.");
mysql_query( "SET NAMES utf8", $db_conn );
mysql_set_charset('utf8', $db_conn);
//mysql_query( "SET CHARACTER SET utf8", $db_conn );

?>