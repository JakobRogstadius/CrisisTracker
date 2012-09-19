<?php
include 'EpiCurl.php';
include 'EpiOAuth.php';
include 'EpiTwitter.php';
include 'secret.php';

$twitterObj = new EpiTwitter($consumer_key, $consumer_secret, $_COOKIE['oauth_token'], $_COOKIE['oauth_token_secret']);

$twitterInfo= $twitterObj->get_statusesFriends();
echo "<h1>Your friends are</h1><ul>";
foreach($twitterInfo as $friend) {
  echo "<li><img src=\"{$friend->profile_image_url}\" hspace=\"4\">{$friend->screen_name}</li>";
}
echo "</ul>";
?>
