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

header('Content-Type: text/html; charset=UTF-8' );
mb_internal_encoding('UTF-8' );

include('common_functions.php');
include('../twitteroauth/login.php');

if (!is_logged_in()) {
  exit("used not logged in");
}

//Validate inputs
if (!isset($_POST['storyid']))
  exit("missing storyid");

$storyID = intval($_POST['storyid']);
$ip = $_SERVER['REMOTE_ADDR'];
$userID = get_user_id();

//Update custom title
if (!isset($_POST['customtitle'])) {
  exit('missing customtitle');
}


include('open_db.php');
$customTitle = urlencode($_POST['customtitle']);
if ($customTitle == '') {
  mysql_query("call AddRemoveStoryCustomTitle($userID, INET_ATON('$ip'), $storyID, null);", $db_conn);
}
else {
  mysql_query("call AddRemoveStoryCustomTitle($userID, INET_ATON('$ip'), $storyID, '$customTitle');", $db_conn);
}
include('close_db.php');

/*
if($mysqli = new mysqli('127.0.0.1', 'jakob', 'the_jak0b$', 'jakob'))
{
  if($stmt = $mysqli->prepare("call AddRemoveStoryCustomTitle($userID, INET_ATON('$ip'), $storyID, ?);"))
  {
    $mysqli->set_charset("utf8");
    $heart = '♥';
    if($stmt->bind_param('s', $heart))// $_POST['customtitle']))
    {
      $ret = $stmt->execute();
    }
    $stmt->close();
  }
}
$mysqli->close();
*/




echo $customTitle;
?>