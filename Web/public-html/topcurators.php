<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
include('header_end.php');
?>

<div class="fullwidth-column">
  <div class="gui-panel">
  <h1>Top curators</h1>
  <table style="border-spacing: 4px; border-collapse: separate; text-align: right;">
    <tr>
      <td style="text-align: left;">&nbsp;</td>
      <td style="width: 50px;" title="Total number of stories curated">Stories</td>
      <td style="width: 50px;" title="Total curation actions">Actions</td>
      <td style="width: 50px;" title="Number of days the curator has been active">Days</td>
      <td style="width: 35px;" title="Stories merged"><img src="img/merge.png" /></td>
      <td title="Stories split"><img src="img/split.png" /></td>
      <td style="width: 35px;" title="Category tags added"><img src="img/tag_category.png" /></td>
      <td title="Category tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_category.png" /></td>
      <td style="width: 35px;" title="Location tags added"><img src="img/tag_location.png" /></td>
      <td title="Location tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_location.png" /></td>
      <td style="width: 35px;" title="Named entity tags added"><img src="img/tag_entity.png" /></td>
      <td title="Named entity tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_entity.png" /></td>
      <td style="width: 35px;" title="Keyword tags added"><img src="img/tag_keyword.png" /></td>
      <td title="Keyword tags removed"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_keyword.png" /></td>
      <td style="width: 35px;" title="Stories hidden"><img src="img/tag_trash.png" /></td>
      <td title="Stories shown"><img src="img/deleted.png" style="position: absolute;"/><img src="img/tag_trash.png" /></td>
    </tr>
<?php
include('api/open_db.php');

$result = mysql_query(
"select 
    UserID as UserID,
    count(distinct StoryID) as StoryCount, 
    count(*) as ActionCount,
    count(distinct date(Timestamp)) as DaysActive,
    sum(EventType=12) as 'Merge',
    sum(EventType=13) as Split,
    sum(EventType=14) as Hide,
    sum(EventType=15) as 'Show',
    sum(EventType=30) as AddKeyword,
    sum(EventType=40) as DelKeyword,
    sum(EventType=31) as AddCategory,
    sum(EventType=41) as DelCategory,
    sum(EventType=32) as AddEntity,
    sum(EventType=42) as DelEntity,
    sum(EventType=33) as AddLocation,
    sum(EventType=43) as DelLocation    
from 
    StoryLog
where EventType!=20 and UserID!=0
group by UserID
order by StoryCount desc, ActionCount desc
;", $db_conn);

while($row = mysql_fetch_array($result)) {
  
  $name = ($row['UserID'] == getUserID() ? getUserName() : "Anonymous");
  
  echo '<tr>';
  echo '<td style="text-align: left;">' . $name . '</td>';
  echo '<td>' . $row['StoryCount'] . '</td>';
  echo '<td>' . $row['ActionCount'] . '</td>';
  echo '<td>' . $row['DaysActive'] . '</td>';
  echo '<td>' . $row['Merge'] . '</td>';
  echo '<td>' . $row['Split'] . '</td>';
  echo '<td>' . $row['AddCategory'] . '</td>';
  echo '<td>' . $row['DelCategory'] . '</td>';
  echo '<td>' . $row['AddLocation'] . '</td>';
  echo '<td>' . $row['DelLocation'] . '</td>';
  echo '<td>' . $row['AddEntity'] . '</td>';
  echo '<td>' . $row['DelEntity'] . '</td>';
  echo '<td>' . $row['AddKeyword'] . '</td>';
  echo '<td>' . $row['DelKeyword'] . '</td>';
  echo '<td>' . $row['Hide'] . '</td>';
  echo '<td>' . $row['Show'] . '</td>';
  echo '</tr>';
}

include('api/close_db.php');
?>
    </table>
  </div>
</div>

<?php
include('footer.php');
?>