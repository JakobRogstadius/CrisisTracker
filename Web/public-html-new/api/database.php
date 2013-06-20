<?php

function get_mysql_connection() {
    $conn = new mysqli("localhost", "USERNAME", "PASSWORD") or die("Database error.");
    $conn->select_db("CrisisTracker");
    $conn->set_charset("utf8");
    return $conn;
}

?>