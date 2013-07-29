<?php
function getTimeParam($name) {
  date_default_timezone_set("UTC");
  if (key_exists($name, $_REQUEST)) {
    $t = strtotime($_REQUEST[$name]);
    if ($t)
      return date("Y-m-d H:i:s", $t);
    else
      return null;
  }
}
function getSafeTextParam($name, $conn) {
  if (key_exists($name, $_REQUEST)) {
    $t = $_REQUEST[$name];
    return $conn->real_escape_string($t);
  }
  return null;
}
function getIntParam($name, $min, $max, $default) {
  if (key_exists($name, $_REQUEST)) {
    $x = intval($_REQUEST[$name]);
    if ($x >= $min && $x <= $max)
        return intval($x);
  }
  return $default;
}
function stemMany($words) {
  $stemmed = array();
  for ($i=0; $i<sizeof($words); $i++)
    $stemmed[$i] = stem($words[$i]);
  return $stemmed;
}
function stem($word) {
  if (strlen($word) < 4 || substr_compare($word,"#",0,1) == 0)
    return $word;
  $before = $word;
  while (($word = preg_replace("/(es|ed|s|ing|ly|n)$/", "", $word)) != $before)
    $before = $word;
  return $word;
}
function isLoggedIn() {
  return true;
}
function getUserID() {
  return 42;
}
?>