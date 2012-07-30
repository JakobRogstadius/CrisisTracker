<?php
session_start();
	if(isset($_POST['login_name'])){
		$_SESSION['name'] = $_POST['login_name'];
	}
	$_SESSION['teste'] = 'hello';
	$_GET['test'] = "jonhy";
	echo json_encode($_SESSION);
?>