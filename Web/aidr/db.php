<!--
      This is a template of your database configuration file. You need to copy it to 'db.php' and edit
      to match your database parameters.
-->

<?php
function getMySqlConnection() {
  $con = mysqli_connect("DB-HOST-HERE","DB-USER-HERE","DB-PASS-HERE","DB-NAME-HERE");

  if (mysqli_connect_errno($con)) {
    echo "Failed to connect to MySQL server: ";
    mysqli_connect_error();
    die();
  }

  $con->query("set names utf8");
  $con->set_charset('utf8');

  return $con;
}
?>
