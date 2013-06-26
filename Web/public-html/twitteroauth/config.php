<?php

/**
 * @file
 * A single location to store configuration.
 */
include_once(dirname(__FILE__) . '/../api/common_functions.php');

define('CONSUMER_KEY', '*');
define('CONSUMER_SECRET', '*');
define('OAUTH_CALLBACK', $SITEURL . '/twitteroauth/callback.php');
