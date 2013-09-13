<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

$SITEURL = 'http://ufn.virtues.fi/crisistracker';
$FEEDURL = $SITEURL . '/atom/top-20-stories.php';

header("Content-Type: application/atom+xml; charset=UTF-8");
//header("Content-Type: text/xml; charset=UTF-8");
mb_internal_encoding( 'UTF-8' );

$tweets = array();
try {
  include '../api/database.php';
  $conn = get_mysql_connection();
  
  $rssfeed = '<?xml version="1.0" encoding="UTF-8"?>';
  $rssfeed .= '<feed xmlns="http://www.w3.org/2005/Atom"
    xmlns:georss="http://www.georss.org/georss"
    xml:base="' . $SITEURL . '">';
  $rssfeed .= "<id>$FEEDURL</id>";
  $rssfeed .= '<title>CrisisTracker RSS feed - Syrian uprising - Largest stories</title>';
  $rssfeed .= '<updated>' . gmdate("c", time()) . '</updated>';
  $rssfeed .= '<subtitle>Most shared stories among top users, first posted in the past 18 hours</subtitle>';
  $rssfeed .= '<link href="' . $FEEDURL . '" rel="self" type="application/rss+xml" />';
  $rssfeed .= '<author><name>Jakob Rogstadius</name><email>jakob.rogstadius@m-iti.org</email></author>';

  $query = "
    select * from (
      select
        Story.StoryID,
        UserCount,
        round(WeightedSize) WeightedSize,
        StartTime,
        EndTime,
        Title
      from Story
      where not IsHidden and Story.StartTime > (select StartTime - interval 18 hour from Story order by StoryID desc limit 1)
      group by Story.StoryID
      order by TopUserCount desc, WeightedSize desc
      limit 20) T
    order by StartTime desc";
  
  $conn = get_mysql_connection();
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($query);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");  
  
  while($row = $result->fetch_object()) {
      $storyID = $row->StoryID;
      $title = $row->Title;
      $size = $row->WeightedSize;
      $startTime = $row->StartTime;
      $endTime = $row->EndTime;
      $topUserCountRecent = $row->UserCount;

      $rssfeed .= '<entry>';
      $rssfeed .= "<id>$SITEURL/index.html?story=" . $storyID . '</id>';
      $rssfeed .= '<title>' . $title . '</title>';
      $rssfeed .= '<link href="' . $SITEURL . '/?story=' . $storyID . '" />';
      $rssfeed .= '<published>' . date("c", strtotime($startTime)) . '</published>';
      $rssfeed .= '<updated>' . date("c", strtotime($endTime)) . '</updated>';
      $rssfeed .= '<summary>' . $title . '</summary>';
      if (intval($geoCount) > 0) {
        $tagArr = explode(';', $geotags);
        foreach ($tagArr as $tag) {
          $rssfeed .= '<georss:point>' . $tag . '</georss:point>';
        }
      }
      $rssfeed .= '</entry>';
  }

  $rssfeed .= '</feed>';
 
  $conn->close();

  echo $rssfeed;
} catch (Exception $ex) {
  echo $ex->getMessage();
}
?>