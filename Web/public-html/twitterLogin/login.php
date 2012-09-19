<?php
include 'EpiCurl.php';
include 'EpiOAuth.php';
include 'EpiTwitter.php';
include 'secret.php';

function printLogin() {
  $twitterObj = null;
  try {
    if (isLoggedIn())
    {
      /*$twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret(), $_COOKIE['oauth_token'], $_COOKIE['oauth_token_secret']);
      $twitterInfo= $twitterObj->get_accountVerify_credentials();
      echo '<a href="userprofile.php?userid=' . $twitterInfo->id . '">' . $twitterInfo->name . '</a>&nbsp;<a href="twitterLogin/logout.php">Log out</a>';*/
      echo '<a href="userprofile.php?userid=' . $_COOKIE['twitter_id'] . '">' . $_COOKIE['twitter_name'] . '</a>&nbsp;<a href="twitterLogin/logout.php">Log out</a>';
    }
    else
    {
      $twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret());
      echo '<a href="' . $twitterObj->getAuthenticateUrl() . '">Log in with Twitter</a>';
    }
  } catch (EpiOAuthException $e) {
    echo $e->getMessage() . ' [<a href="twitterLogin/logout.php">Log out</a>]';
  }
}

function printLoginShort() {
  $twitterObj = null;
  try {
    if (isLoggedIn())
    {
      //$twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret(), $_COOKIE['oauth_token'], $_COOKIE['oauth_token_secret']);
      //$twitterInfo= $twitterObj->get_accountVerify_credentials();
      echo '<a href="twitterLogin/logout.php">log out</a>';
    }
    else
    {
      $twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret());
      echo '<a href="' . $twitterObj->getAuthenticateUrl() . '">log in</a>';
    }
  } catch (EpiTwitterException $e) {
    echo $e->getMessage();
  }
}

function setLoginRedirect() {
    setcookie('login_redirect', $_SERVER['REQUEST_URI'], 0, '/');
}

function isLoggedIn() {
  return isset($_COOKIE['oauth_token']) && isset($_COOKIE['oauth_token_secret']);
}

function getUserID($forceLookup = FALSE) {
  if (isLoggedIn()) {
    if ($forceLookup) {
      $twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret(), $_COOKIE['oauth_token'], $_COOKIE['oauth_token_secret']);
      $twitterInfo= $twitterObj->get_accountVerify_credentials();
      return $twitterInfo->id;
    }
    else {
      return $_COOKIE['twitter_id'];
    }
  }
  else {
    return null;
  }
}

function getUserName($forceLookup = FALSE) {
  if (isLoggedIn()) {
    if ($forceLookup) {
      $twitterObj = new EpiTwitter(getConsumerKey(), getConsumerSecret(), $_COOKIE['oauth_token'], $_COOKIE['oauth_token_secret']);
      $twitterInfo= $twitterObj->get_accountVerify_credentials();
      return $twitterInfo->name;
    }
    else {
      return $_COOKIE['twitter_name'];
    }
  }
  else {
    return null;
  }
}
?>