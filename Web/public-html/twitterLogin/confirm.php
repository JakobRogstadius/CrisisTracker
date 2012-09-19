<?php
include 'EpiCurl.php';
include 'EpiOAuth.php';
include 'EpiTwitter.php';
include 'secret.php';

$twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret());

$twitterObj->setToken($_GET['oauth_token']);
$token = $twitterObj->getAccessToken();
$twitterObj->setToken($token->oauth_token, $token->oauth_token_secret);
$twitterInfo= $twitterObj->get_accountVerify_credentials();

// save to cookies
setcookie('oauth_token', $token->oauth_token, 0, '/');
setcookie('oauth_token_secret', $token->oauth_token_secret, 0, '/');
setcookie('twitter_id', $twitterInfo->id, 0, '/');
setcookie('twitter_name', $twitterInfo->name, 0, '/');

include '../api/open_db.php';
mysql_query("insert into User (TwitterUserID, Name) values (" . $twitterInfo->id . ",'" . $twitterInfo->name . "') on duplicate key update Name=VALUES(Name);", $db_conn);
include '../api/close_db.php';

if (isset($_COOKIE['login_redirect']))
  header('Location: ' . $_COOKIE['login_redirect']) ;
else
  header('Location: ../index.php') ;
?>
