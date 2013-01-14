<?php
include '../api/common_functions.php';

setcookie("oauth_token", "", time() - 3600, "$SITEPATH/");
setcookie("oauth_token_secret", "", time() - 3600, "$SITEPATH/");
setcookie("twitter_id", "", time() - 3600, "$SITEPATH/");
setcookie("twitter_name", "", time() - 3600, "$SITEPATH/");

if (isset($_COOKIE['login_redirect']))
  header('Location: ' . $_COOKIE['login_redirect']) ;
else
  header('Location: ../index.php') ;
?>

