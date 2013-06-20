<?php
/*
INPUT:
  storyid: long
  customtitle: string

OUTPUT:
  1 if success
*/

function getSafeValues($input, $readNumbers = FALSE) {
  $output = array();
  if (is_array($input)) {
    foreach($input as $tag) {
      $tmp = '';
      if ($readNumbers)
        $tmp = intval($tag);
      else
        $tmp = stripMaliciousSql($tag);
      if ($tmp != '')
        $output[] = $tmp;
    }
  }
  return $output;
}

ini_set('display_errors', 1);
ini_set('log_errors', 1);
ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt');
error_reporting(E_ALL);

//header('Content-Type: text/html; charset=UTF-8' );
//mb_internal_encoding('UTF-8' );

include('common_functions.php');
include('../twitteroauth/login.php');

if (!is_logged_in()) {
  exit("used not logged in");
}

//Validate inputs
if (isset($_GET['isanonymous'])) {
  $userID = get_user_id(TRUE);
  $userName = get_user_name(TRUE);
  $isAnon = max(0, min(1, intval($_GET['isanonymous'])));

  include('open_db.php');
  $sql = "insert into User (TwitterUserID, Name, IsAnonymous) values ($userID, '$userName', $isAnon) on duplicate key update Name=values(Name), IsAnonymous=values(IsAnonymous);";
  //echo $sql;
  mysql_query($sql, $db_conn);
  include('close_db.php');
}

header('Location: ' . $_SERVER['HTTP_REFERER'] );
?>