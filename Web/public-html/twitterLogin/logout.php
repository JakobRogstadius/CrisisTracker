<?php
setcookie("oauth_token", "", time() - 3600, '/');
setcookie("oauth_token_secret", "", time() - 3600, '/');

if (isset($_COOKIE['login_redirect']))
  header('Location: ' . $_COOKIE['login_redirect']) ;
else
  header('Location: ../index.php') ;
?>

