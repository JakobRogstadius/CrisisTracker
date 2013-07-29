<?php
/**
 * @file
 * Clears PHP sessions and redirects to the connect page.
 */

/* Load and clear sessions */
session_start();
$redirect = $_SESSION['login_redirect'];
session_destroy();

/* Redirect to page with the connect to Twitter option. */
header('Location: ' . $redirect);
