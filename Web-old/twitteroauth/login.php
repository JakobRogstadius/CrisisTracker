<?php
//session_start();
require_once('twitteroauth/twitteroauth.php');
require_once('config.php');

function is_logged_in() {
    return !(empty($_SESSION['access_token'])
        || empty($_SESSION['access_token']['oauth_token'])
        || empty($_SESSION['access_token']['oauth_token_secret']));
}

function get_user_name() {
    if (!is_logged_in())
        return null;

    return $_SESSION['access_token']['screen_name'];
}

function get_user_id() {
    if (!is_logged_in())
        return null;

    return $_SESSION['access_token']['user_id'];
}

function print_login_button() {
    if (is_logged_in())
        echo '<a href="userprofile.php?userid='.get_user_id().'">'. get_user_name().'</a> | <a href="./twitteroauth/clearsessions.php">Log out</a>';
    else
        echo '<a href="./twitteroauth/redirect.php">Sign in with Twitter</a>';
}

function print_login_short() {
    if (is_logged_in())
        echo '<a href="./twitteroauth/clearsessions.php">log out</a>';
    else
        echo '<a href="./twitteroauth/redirect.php">log in</a>';
}

function set_login_redirect($uri, $scope) {
    $_SESSION["login_redirect"] = $uri;
}