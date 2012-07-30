<?php
/*
INPUT:
  sortorder, one of:
    weighted
    recent
    old
    large
    active
  limit: int

weighted: least(UserCount, TweetCount + 0.5*log(10+RetweetCount)) / log(2 + AgeInMinutes)
recent:   age
old:      -age
large:    least(UserCount, TweetCount + 0.5*log(10+RetweetCount))
active:	  least(UserCountRecent, TweetCountRecent + 0.5*log(10+RetweetCountRecent))

OUTPUT: 
  Array of stories, containing subset of fields:
    StoryID
    Title
    UserCount
    StartTime
    LocationCount
    CategoryCount
    EntityCount
    KeywordCount
*/

ini_set('display_errors', 1); 
ini_set('log_errors', 1); 
ini_set('error_log', dirname(__FILE__) . '/php_error_log.txt'); 
error_reporting(E_ALL);

header( 'Content-Type: text/xml; charset=UTF-8' );
mb_internal_encoding( 'UTF-8' );

include('common_functions.php');
include('open_db.php');

$sortOrder = 'weighted';
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
}
$limit = 20;
if (isset($_GET['limit'])) {
  $limit = intval($_GET['limit']);
}

if ($limit < 1 || $limit > 100)
  $limit = 20;
$orderby = '';
if ($sortOrder == 'active')
  $orderby = 'ImportanceRecent';
else //weighted
  $orderby = 'TaggingImportance / log(2 + (unix_timestamp(utc_timestamp()) - unix_timestamp(min(tc.StartTime))) / 60)';


// Story info
$querySql = "select
    s.StoryID, Title, Importance, StartTime,
    count(distinct TagID) as LocationCount,
    count(distinct InfoCategoryID) as CategoryCount,
    count(distinct InfoEntityID) as EntityCount,
    count(distinct InfoKeywordID) as KeywordCount
from (
        select 
            StoryID,
            Title,
            round(exp(Importance)) as Importance,
            StartTime,
            $orderby as score
        from Story
        order by score desc
        limit $limit
    ) s
    left join StoryInfoCategoryTag c on c.StoryID=s.StoryID
    left join StoryInfoEntityTag e on e.StoryID=s.StoryID
    left join StoryInfoKeywordTag k on k.StoryID=s.StoryID
    left join StoryLocationTag l on l.StoryID=s.StoryID
group by s.StoryID
order by score desc";

//echo $querySql;
$storyInfoResult = mysql_query($querySql, $db_conn);

$stories = array();
while($row = mysql_fetch_array($storyInfoResult)) {
  $story = array();
  $story['storyID'] = $row['StoryID'];
  $story['title'] = htmlspecialchars($row['Title']);
  $story['userCount'] = $row['Importance'];
  $story['startTime'] = $row['StartTime'];
  $story['locationCount'] = $row['LocationCount'];
  $story['entityCount'] = $row['EntityCount'];
  $story['categoryCount'] = $row['CategoryCount'];
  $story['keywordCount'] = $row['KeywordCount'];
  $stories[] = $story;
}

//Print
echo array_to_xml($stories, new SimpleXMLElement('<stories/>'))->asXML();

include('close_db.php');
?>