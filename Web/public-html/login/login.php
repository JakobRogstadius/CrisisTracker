<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

session_start();
	if(isset($_POST['login_name'])){
		$_SESSION['name'] = $_POST['login_name'];
	}
	$_SESSION['teste'] = 'hello';
	$_GET['test'] = "jonhy";
	echo json_encode($_SESSION);
?>