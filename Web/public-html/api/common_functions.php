<?php

$SITEURL = 'http://ufn.virtues.fi/crisistracker';
  
function addLinksToText($text) {
  $text = preg_replace('/http\S+/', '<a href="$0" target="_blank">$0</a>', $text);
  return preg_replace('/@[A-Za-z0-9_]+/', '<a href="https://twitter.com/#!/$0" target="_blank">$0</a>', $text);
}

function stripMaliciousSql($str)
{
  $str = str_replace("\\", "", $str);
  $str = str_replace(";", "", $str);
  $str = str_replace("drop", "", $str);
  $str = str_replace("table", "", $str);
  $str = str_replace("insert", "", $str);
  $str = str_replace("create", "", $str);
  $str = str_replace("truncate", "", $str);
  $str = str_replace("alter", "", $str);
  $str = str_replace("'", "", $str);
  //$str = str_replace('"', "", $str);
  $str = str_replace("\r", "", $str);
  $str = str_replace("\x00", "", $str);
  $str = str_replace("\x1a", "", $str);
  return $str;
}

function array_to_xml(array $arr, SimpleXMLElement $xml)
{
  try {
    foreach ($arr as $k => $v) {
        $kk = $k;
        if(is_numeric($k))
            $kk = 'i-'.$k;
        is_array($v)
            ? array_to_xml($v, $xml->addChild($kk))
            : $xml->addChild($kk, str_replace('&', '&amp;', $v));
    }
  } catch (Exception $e) {
  }
  return $xml;
}

function dumpTable($tableName) {
  include('api/open_db.php');
  
  $tableName = stripMaliciousSql($tableName);
  //Percent of tweets tagged
  $result = mysql_query("select * from $tableName order by 1 limit 1000", $db_conn);
  
  echo '<table style="border-spacing: 4px; border-collapse: separate">';
  $firstRow = true;
  while($row = mysql_fetch_array($result)) {
    if ($firstRow) {
      echo '<tr>';
      foreach ($row as $key => $value) {
        if (!is_int($key))
          echo "<td><strong>$key</strong></td>";
    }
    echo '</tr>';
    $firstRow = false;
    }
    
    echo '<tr>';
    foreach ($row as $key => $value) {
      if (!is_int($key)) {
        if (ord($value) == 1)
          echo "<td>yes</td>";
        elseif (ord($value) == 0)
          echo "<td>no</td>";
        else
          echo "<td>" . htmlspecialchars($value) . "</td>";
        }
    }
    echo '</tr>';
  }
  echo '</table>';
  
  include('api/close_db.php');
}

?>