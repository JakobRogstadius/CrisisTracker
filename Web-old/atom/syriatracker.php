<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

  $SITEURL = 'http://ufn.virtues.fi/crisistracker';
  $FEEDURL = $SITEURL . '/atom/syriatracker.php';

  header("Content-Type: application/atom+xml; charset=UTF-8");
  //header("Content-Type: text/xml; charset=UTF-8");
  mb_internal_encoding( 'UTF-8' );

  include('../api/common_functions.php');
  include('../api/open_db.php');
  
  $rssfeed = '<?xml version="1.0" encoding="UTF-8"?>';
  $rssfeed .= '<feed xmlns="http://www.w3.org/2005/Atom"
    xmlns:georss="http://www.georss.org/georss"
    xml:base="http://ufn.virtues.fi/crisistracker">';
  $rssfeed .= "<id>$FEEDURL</id>";
  $rssfeed .= '<title>CrisisTracker RSS feed - Syrian uprising - Largest stories</title>';
  $rssfeed .= '<updated>' . gmdate("c", time()) . '</updated>';
  $rssfeed .= '<subtitle>Most shared stories among top users, first posted in the past 24 hours</subtitle>';
  $rssfeed .= '<link href="' . $FEEDURL . '" rel="self" type="application/rss+xml" />';
  $rssfeed .= '<author><name>Jakob Rogstadius</name><email>jakob@m-iti.org</email></author>';

  $query = "select 
        T.*,
        count(distinct cat.InfoCategoryID) CategoryCount,
        group_concat(distinct catLabel.Category order by catLabel.InfoCategoryID separator ',') as Categories,
        count(distinct loc.TagID) LocationCount,
        group_concat(distinct concat(Longitude, ' ', Latitude) separator ';') as LocationTags
    from (
            select
                Story.StoryID,
                Title,
                round(exp(Importance)) as Size,
                StartTime,
                EndTime,
                TopUserCountRecent
            from Story
            where StartTime > utc_timestamp() - interval 24 hour
            group by Story.StoryID
            order by TopUserCount desc
            limit 10
        ) T
        left join StoryLocationTag loc on loc.StoryID=T.StoryID
        left join StoryInfoCategoryTag cat on cat.StoryID=T.StoryID
        left join InfoCategory catLabel on catLabel.InfoCategoryID = cat.InfoCategoryID
    group by T.StoryID
    order by StartTime desc";
  $result = mysql_query($query) or die ("Could not execute query");

  while($row = mysql_fetch_array($result)) {
      $storyID = $row['StoryID'];
      $title = $row['Title'];
      $size = $row['Size'];
      $startTime = $row['StartTime'];
      $endTime = $row['EndTime'];
      $topUserCountRecent = $row['TopUserCount'];
      $geoCount = $row['LocationCount'];
      $geoTags = $row['LocationTags'];

      $rssfeed .= '<entry>';
      $rssfeed .= "<id>$SITEURL/story.php?storyid=" . $storyID . '</id>';
      $rssfeed .= '<title>@syriatracker ' . $title . '</title>';
      $rssfeed .= '<link href="' . $SITEURL . '/story.php?storyid=' . $storyID . '" />';
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
 
  include('../api/close_db.php');

  echo $rssfeed;
?>