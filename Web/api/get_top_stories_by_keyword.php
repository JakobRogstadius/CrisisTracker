<?php
header('Access-Control-Allow-Origin: *');
include_once 'common.php';

$tweets = array();
try {
  include 'database.php';
  $conn = get_mysql_connection();

  $mintime = getTimeParam("mintime");
  $maxtime = getTimeParam("maxtime");
  $textfilter = getSafeTextParam("text", $conn);

  $where = '';
  if ($mintime != null && $maxtime != null) {
    $where .= " and Story.StartTime between '$mintime' and '$maxtime' ";
  }

  $storySelectTextFilter = '';
  $keywordSelectTextFilter = '';
  if ($textfilter != null) {
    if (strlen($where) > 0)
      $where .= ' and ';
    $textfilter = preg_replace("/ *, */", ",", $textfilter);
    $words = stemMany(preg_split("/,| |;/", $textfilter, -1, PREG_SPLIT_NO_EMPTY));
    $textfilter = "'" . implode("','", $words) . "'";

    $storySelectTextFilter =
      "join StoryInfoKeywordTag sikt on sikt.StoryID=Story.StoryID
      join InfoKeyword ik on ik.InfoKeywordID=sikt.InfoKeywordID";

    $where .= " and ik.Keyword in ($textfilter)";

    $keywordSelectTextFilter =
      "join StoryInfoKeywordTag sikt2 on sikt2.StoryID=sikt1.StoryID
      join InfoKeyword ik2 on ik2.InfoKeywordID=sikt2.InfoKeywordID and ik2.Keyword in ($textfilter)";
  }

  $otherName = 'OTHER';

  //Top 500 stories, with top keyword, size and time
  $sql =
    "select * from (
    select s1.StoryID, s1.Keyword, s2.StartTime, s2.WeightedSize, replace(s2.Title, '\n', ' ') Title,
        @rnum:=if(@sid=s1.StoryID, @rnum+1, 0) rnum,
        @sid:=s1.StoryID sid
    from
        (select @rnum:=-1, @sid:=-1) initvars,
        (
        select StoryID, topic.InfoKeywordID, coalesce(Keyword, '$otherName') Keyword, Filters, max(Weight) Weight
        from
            (select Story.StoryID from Story
            $storySelectTextFilter
            where not IsHidden $where
            order by TopUserCount desc limit 150) story
            natural join StoryInfoKeywordTag tag
            left join (
                select InfoKeywordID, Keyword, group_concat(Word) as Filters, Rank
                from (
                    select ik1.InfoKeywordID, ik1.Keyword, log(count(*))*sum(sikt1.Weight) Rank
                    from StoryInfoKeywordTag sikt1
                    natural join InfoKeyword ik1
                    $keywordSelectTextFilter
                    where ik1.Keyword not like '#%'
                    group by ik1.InfoKeywordID order by Rank desc limit 200
                    ) T
                left join TwitterTrackFilter f on instr(f.Word, T.Keyword) > 0
                group by InfoKeywordID order by Rank desc
                ) topic on topic.InfoKeywordID=tag.InfoKeywordID
            group by StoryID, topic.InfoKeywordID
            order by StoryID, if(topic.InfoKeywordID is null, 1, 2) * Weight desc
        ) s1
        join Story s2 on s2.StoryID=s1.StoryID
    ) T
    having rnum<4";

  $conn = get_mysql_connection();
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
  $result = $conn->query($sql);
  $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

  $stories = array();
  $keywords = array();

  while($row = $result->fetch_object()) {
    $story = array();
    $story["story_id"] = $row->StoryID;
    $story["keyword"] = $row->Keyword;
    $story["weighted_size"] = $row->WeightedSize;
    $story["start_time"] = $row->StartTime;
    $story["title"] = $row->Title;
    $stories[] = $story;

    $keywords[] = $row->Keyword;
  }

  //Limit to top 20 keywords
  $keywordFq = array_count_values($keywords);
  arsort($keywordFq);
  $keywordFq = array_slice($keywordFq, 0, 20, true);

  for($i=0; $i<sizeof($stories); $i++) {
    if (!array_key_exists($stories[$i]["keyword"], $keywordFq)) {
      $stories[$i]["keyword"] = $otherName;
    }
  }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();
echo json_encode($stories);
?>