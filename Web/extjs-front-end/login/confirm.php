<?php
include 'EpiCurl.php';
include 'EpiOAuth.php';
include 'EpiTwitter.php';
include 'secret.php';
include '../data/common_functions.php';

$twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret());

$twitterObj->setToken($_GET['oauth_token']);
$token = $twitterObj->getAccessToken();
$twitterObj->setToken($token->oauth_token, $token->oauth_token_secret);
$twitterInfo= $twitterObj->get_accountVerify_credentials();

// save to cookies
setcookie('oauth_token', $token->oauth_token, 0, "$SITEPATH/");
setcookie('oauth_token_secret', $token->oauth_token_secret, 0, "$SITEPATH/");
setcookie('twitter_id', $twitterInfo->id, 0, "$SITEPATH/");
setcookie('twitter_name', $twitterInfo->name, 0, "$SITEPATH/");

include '../data/open_db.php';
mysql_query("insert into User (TwitterUserID, Name) values (" . $twitterInfo->id . ",'" . $twitterInfo->name . "') on duplicate key update Name=VALUES(Name);", $db_conn);
include '../data/close_db.php';

if (isset($_COOKIE['login_redirect']))
  header('Location: ' . $_COOKIE['login_redirect']) ;
else
  header('Location: ../index.php') ;
?>
