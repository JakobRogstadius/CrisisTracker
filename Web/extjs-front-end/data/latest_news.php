<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

function naiveStemming($words)
{
    for ($i=0; $i<count($words); $i++)
    {
        if (strlen($words[$i]) < 4 || substr_compare($words[$i],"#",0,1) == 0)
            return str;
        $before = $words[$i];
        while (($words[$i] = preg_replace("/(es|ed|s|ing|ly|n)$/", "", $words[$i])) != $before)
            $before = $words[$i];
    }
    return $words;
}

function getSafeValues($input, $readNumbers = FALSE)
{
    $output = array();
    if (is_array($input))
    {
        foreach($input as $tag)
        {
            $tmp = '';
            if ($readNumbers)
                $tmp = intval($tag);
            else
                $tmp = stripMaliciousSql($tag);
            if ($tmp != '')
                $output[] = $tmp;
        }
    }
    return $output;
}



//WHERE
$where = "where not IsHidden and psm.StoryID2 is null ";
$wherejoins = "";
$hitcounts = "";
$having = "having ";

$storycount = '10';
if (isset($_GET['storycount'])) { $storycount = max(1,min(200, intval($_GET['storycount']))); }
$hours = '8';
if (isset($_GET['hours'])) { $hours = max(1,min(30*24, intval($_GET['hours']))); }

$where = "";
$wherejoin = "";
$hitcounts = "";
$having = "";
$useAND = 1;
//Keyword filter
if (isset($_GET['keywordfilter']))
{
    $keywords = naiveStemming(getSafeValues(explode(',', $_GET['keywordfilter'])));
    if (sizeof($keywords) > 0)
    {
        if (isset($_GET['keywordoperator']))
        {
            if ($_GET['keywordoperator'] == 'or')
                $useAND=0;
        }
      
        $where .= "and InfoKeywordID in (select distinct InfoKeywordID from InfoKeyword where Keyword in ('" . strtolower(implode("','", $keywords)) . "'))";
        $wherejoin .= "join StoryInfoKeywordTag keyword on keyword.StoryID=s.StoryID";
        if ($useAND)
        {
            $hitcounts .= ",count(distinct keyword.InfoKeywordID) KeywordHitCount";
            $having .= "having KeywordHitCount=" . sizeof($keywords);
        }
    }
}


$sqlQuery = "select
    T.*,
    count(distinct TagID) as LocationCount,
    count(distinct InfoCategoryID) as CategoryCount,
    count(distinct InfoEntityID) as EntityCount,
    count(distinct InfoKeywordID) as KeywordCount
    from (
    select
        s.StoryID,
        StartTime as StartTimeRaw,
        ShortDate(StartTime) as StartTime,
        ceil(exp(Importance)) as Popularity,
        MaxGrowth,
        Title,
        CustomTitle
        $hitcounts
    from Story s
        left join PendingStoryMerges psm on psm.StoryID2=s.StoryID
        left join StoryCustomTitle sct on sct.StoryID=s.StoryID
        $wherejoin
    where
        StartTime > utc_timestamp()-interval $hours hour
        and psm.StoryID1 is null
        $where
    group by s.StoryID
    $having
    order by MaxGrowth desc 
    limit $storycount
) T
left join StoryInfoCategoryTag c on c.StoryID=T.StoryID
left join StoryInfoEntityTag e on e.StoryID=T.StoryID
left join StoryInfoKeywordTag k on k.StoryID=T.StoryID
left join StoryLocationTag l on l.StoryID=T.StoryID
group by T.StoryID
order by StartTimeRaw desc;";

//echo $sqlQuery; exit();

include('open_db.php');
$storyInfoResult = mysql_query($sqlQuery, $db_conn);

$output = array();
$output['success'] = true;
$output['stories'] = array();
while($row = mysql_fetch_array($storyInfoResult))
{
    $output['stories'][] =
        array(
            'story_id' => $row['StoryID'],
            'start_time' => $row['StartTimeRaw'],
            'start_time_str' => $row['StartTime'],
            'popularity' => intval($row['Popularity']),
            'max_growth' => intval($row['MaxGrowth']),
            'title' => htmlspecialchars($row['Title']),
            'custom_title' => htmlspecialchars(urldecode($row['CustomTitle'])),
            'location_count' => intval($row['LocationCount']),
            'category_count' => intval($row['CategoryCount']),
            'entity_count' => intval($row['EntityCount']),
            'keyword_count' => intval($row['KeywordCount'])
        );
}

include('close_db.php');

echo json_encode($output);
?>