<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include_once('api/common_functions.php');
include_once('twitterLogin/login.php');
include('header_start.php');

?>
<style>
  .question-table tr {
    border-bottom: 1px solid #ccc;
  }
  .question-table td {
    vertical-align: top;
  }
  .question-text {
    width: 400px;
    padding: 8px 10px 8px 10px;
  }
  .op {
    width: 25px;
    text-align: center;
  }
  .color0 { }
  .color1 { background-color: #f0f0f0; }
  .color2 { background-color: #e0e0e0; }
  .op-middle { border-right: 1px dotted #ccc; }
  .na {
    padding-left: 10px;
    padding-right: 10px;
    text-align: center;
  }
  .comment-field {
    width: 220px;
  }
  textarea {
    width: 450px;
    margin: 5px;
    height: 70px;
  }
  .question-table table {
    margin: 3px;
  }
  .question-table table tr {
    border: none;
  }
  .option-list td {
    text-align: center;
    width: 70px;
  }
</style>
<script>
    function isArray(element) {
      return element.length != null;
    }
    
    function hasCheckedItem(nodeList) {
      for (var i=0; i<nodeList.length; i++) {
        if (nodeList[i].checked)
          return true;
      }
      return false;
    }
    
    function validateQuestion(inputElement) {
      if (isArray(inputElement)) {
        if (!hasCheckedItem(inputElement)) {
          inputElement[0].parentNode.parentNode.style.backgroundColor = "#fcc";
          return 0;
        }
        else {
          inputElement[0].parentNode.parentNode.style.backgroundColor = "";
          return 1;
        }        
      }
      else {
        if (inputElement.value == "") {
          inputElement.parentNode.parentNode.style.backgroundColor = "#fcc";
          return 0;
        }
        else {
          inputElement.parentNode.parentNode.style.backgroundColor = "";
          return 1;
        }
      }
    }

    function validateInput() {
      var valid = 1;
      for (var i=0;i<inputFields.length;i++) {
        v = validateQuestion(document.questionnaire.elements[inputFields[i]]);
        console.log(inputFields[i] + ": " + v);
        valid &= v;
      }

      if (!valid) {
        alert("Please answer all questions before submitting your answers.");
        return false;
      }
      else {
        return true;
      }
    }
    
    var inputFields = [
      //"clear-task",
      "ui-learnable",
      "ui-effective",
      "tiring",
      "educational",
      "sense-of-achievement",
      "credit-self",
      "credit-others",
      "teamwork",
      "lang-hard",
      "story-quality",
      "tag-stories-page",
      "tags-hard",
      "merge-hard",
      "accurate-self",
      "accurate-others",
      "detailed-reports",
      "summary-reports",
      "osm-important",
      "ct-impact",
      "overview",
      "meaningful-now",
      "meaningful-future",
      "volunteer-again",
      "worst-comment",
      "best-comment",
      "improvements-comment",
      "twitter-understanding",
      "online-xp",
      "onsite-xp",
      "in-skype-chat",
      "interview"
    ];
</script>

<?
include('header_end.php');

$likertQuestions = array(
// "clear-task" => "My task as a volunteer was well defined and clear.",
 "ui-learnable" => "CrisisTracker is easy to learn to use.",
 "ui-effective" => "I can effectively organize information about a crisis using CrisisTracker.",
 "educational" => "Working in CrisisTracker as a volunteer has improved my understanding of the Syrian conflict.",
 "tiring" => "Working in CrisisTracker as a volunteer is tiring.",
 "sense-of-achievement" => "After finishing tagging and merging several stories in CrisisTracker, I feel a sense of achievement.",
 "credit-self" => "I feel that CrisisTracker helps me get recognition for my volunteering effort.",
 "credit-others" => "I can see how much and in what way other volunteers contribute.",
 "teamwork" => "As a volunteer, I have felt that I was part of a team that together tried to handle the workload.",
 "lang-hard" => "I contributed less because a lot of the content was in a language I don't understand (e.g. Arabic).",
 "story-quality" => "CrisisTracker is good at grouping together related tweets into stories.",
 "tag-stories-page" => "I mainly used the &quot;Tag stories&quot; page (the page with just stories, no filters) to find stories to work on.",
 "tags-hard" => "Knowing which tags to assign to a story is difficult.",
 "merge-hard" => "Knowing which stories to merge is difficult.",
 "accurate-self" => "I tried hard to be accurate when merging and tagging stories.",
 "accurate-others" => "Tags assigned by other volunteers were at least as accurate as mine.",
 "detailed-reports" => "CrisisTracker is good at capturing detailed reports of events on the ground.",
 "summary-reports" => "CrisisTracker is good at capturing high-level summaries and news articles.",
 "overview" => "While using CrisisTracker, I felt I had a good overview of what Twitter users were saying relating to the Syrian conflict.",
 "osm-important" => "During large-scale crisis, it is important for decision makers and responders to be able to use social media (e.g. Twitter and Youtube) as a source of information.",
 "ct-impact" => "By volunteering in CrisisTracker, I can effectively help responders on the ground gain better situation awareness.",
 "meaningful-now" => "Participating in this deployment of CrisisTracker has been meaningful.",
 "meaningful-future" => "Participation in future deployments of CrisisTracker will be meaningful if the data is requested by a humanitarian organization.<br/><i>(e.g. U.N., Red Cross, Amnesty International)</i>",
 "volunteer-again" => "I would like to work as a volunteer in CrisisTracker again during future disasters."
);
$likertCount = 6;


$userID = getUserID(TRUE);
if (is_null($userID)) {
  ?>
    <div class="fullwidth-column">
      <div class="gui-panel textpanel">
        <h1>Log in required</h1>
        <p>Please <?php echo printLoginShort(); ?> to participate in the volunteer survey.</p>
        <p>Your feedback is valuable even if you only curated as little as one story.</p>
      </div>
    </div>
  <?php
  include('footer.php');
  exit;
}

include('api/open_db.php');

$userAnsweredResult = mysql_query("select count(*) as 'answers' from SyriaDeploymentVolunteerSurveyAnswer where TwitterUserID=$userID;");
$resultObj = mysql_fetch_object($userAnsweredResult);

$userHasAnswered = ($resultObj->answers > 0);

if (!$userHasAnswered && !is_null($_POST) && count($_POST) > 0) {
  $answers = array();
  foreach ($_POST as $key => $value) {
    if (is_null($value) || $value=='')
      continue;
    if (substr($key, -7) == 'comment') {
      $qid = substr($key, 0, -8);
      $answers[$qid]->comment = stripMaliciousSql($value);
    }
    else {
      $answers[$key]->value = $value[0];
    }
  }
  
  $sql = "insert ignore into SyriaDeploymentVolunteerSurveyAnswer (TwitterUserID, QuestionID, Timestamp, LikertRange, AnswerValue, AnswerText) values ";
  $firstRow = true;
  foreach ($answers as $qid => $answer) {
    if ($firstRow) $firstRow = false;
    else $sql .= ',';
    
    $value = property_exists($answer, 'value') ? $answer->value : 'null';
    $comment = property_exists($answer, 'comment') ? "'" . $answer->comment . "'" : 'null';
    $likert = array_key_exists($qid, $likertQuestions) ? $likertCount : 'null';
    
    $sql .= "($userID,'$qid',utc_timestamp(),$likert,$value,$comment)";
  }

  //echo "<pre>$sql</pre>";
  mysql_query($sql, $db_conn);
  
  $userHasAnswered = true;
}

include('api/close_db.php');

if ($userHasAnswered) {
?>
  <div class="fullwidth-column">
    <div class="gui-panel textpanel">
      <h1>Thank you!</h1>
      <p>Your answers have been saved.</p>
      <p>Thanks to your help in this evaluation deployment, we now have lots of
      data which will be analyzed to better understand how to prepare and improve
      the CrisisTracker platform for future use in humanitarian crises around the
      world.</p>
      <p>We hope you have enjoyed participating and we hope to see you again in
      future deployments!</p>
    </div>
  </div>
<?php
}
else {
?>
  
  <div class="fullwidth-column">
    <div class="gui-panel textpanel">
      <h1>Survey for CrisisTracker Volunteers</h1>
      <div class="gui-subpanel">
        <p style="width: 600px;">Dear volunteer,
        <br/><br/>
        By filling in this survey, you can contribute information that is very
        helpful to the future development of the CrisisTracker platform. We will
        use your answers to try to remove sources of frustration, make the workflow
        more efficient, improve communication between volunteers and generally
        make the volunteering experience more enjoyable and rewarding.
        <br/><br/>
        Your answers will be connected to your user account so that we can consider
        your feedback together with your activity as a volunteer. We will never
        reveal your identity to anyone outside of the research and development
        team.
        <br/><br/>
        <b>Your feedback is valuable even if you only curated as little as one
        story</b>.
        <br/><br/>
        Best regards,<br/>
        Jakob Rogstadius<br/>
        <i>CrisisTracker Lead developer</i>
        </p>
      </div>
      <br/>
      <form method="post" name="questionnaire" onsubmit="return validateInput()">
        <div class="gui-subpanel">
          <h2>To what extent do you agree with the following statements?</h2>
          <br/>
          <table class="question-table">
            <tr>
              <td><b>Statement</b></td>
              <td colspan="3"><b>Strongly<br/>disagree</b></td>
              <td colspan="3" style="text-align: right"><b>Strongly<br/>agree</b></td>
              <td class="na"><b>Don't know</b></td>
              <td><b>Clarification (optional)</b></td>
            </tr>
            <?php
            $colorindices = array(2,1,0,0,1,2);
            foreach ($likertQuestions as $qid => $question) {
              echo '<tr>';
              echo '<td class="question-text">' . $question . '</td>';
              for ($j=0;$j<$likertCount;$j++) {
                $middle = ($j==2 ? ' op-middle' : '');
                echo '<td class="op color'. $colorindices[$j] . $middle .'"><input type="radio" name="' . $qid . '" value="' . $j . '"/></td>';
              }
              echo '<td class="na"><input type="radio" name="' . $qid . '" value="100"/></td>';
              echo '<td><input class="comment-field" type="text" name="' . $qid . '-comment" /></td>';
              echo '</tr>';
            }
            ?>
          </table>
        </div>
        <br />
        <div class="gui-subpanel">
          <table class="question-table">
            <tr>
              <td class="question-text">As a CrisisTracker volunteer, what parts
              of the work (if any) feel tedious, inefficient, pointless or boring?</td>
              <td><textarea name="worst-comment"></textarea></td>
            </tr>
            <tr>
              <td class="question-text">As a CrisisTracker volunteer, what parts
              of the work (if any) feel interesting, efficient, meaningful or fun?</td>
              <td><textarea name="best-comment"></textarea></td>
            </tr>
            <tr>
              <td class="question-text">What new feature would you most like to see in CrisisTracker?</td>
              <td><textarea name="improvements-comment"></textarea></td>
            </tr>
            <tr>
              <td class="question-text">Are you familiar with how information spreads on Twitter, through tweets, retweets, followers and mentions?</td>
              <td>
                <input type="radio" name="twitter-understanding" id="twitter-understanding-0" value="0" /><label for="twitter-understanding-0">No</label>
                <input type="radio" name="twitter-understanding" id="twitter-understanding-1" value="1" /><label for="twitter-understanding-1">Yes</label>
              </td>
            </tr>
            <tr>
              <td class="question-text">Have you previously volunteered for
              <b>online</b> disaster information management? (e.g. mapping,
              translating or collecting reports)</td>
              <td>
                <input type="radio" name="online-xp" id="online-xp-0" value="0" /><label for="online-xp-0">No</label>
                <input type="radio" name="online-xp" id="online-xp-1" value="1" /><label for="online-xp-1">Yes</label>
              </td>
            </tr>
            <tr>
              <td class="question-text">Are you in way regularly involved in
              physical disaster response? (e.g. search and rescue, victim
              councelling, first aid, technical support or management)</td>
              <td>
                <input type="radio" name="onsite-xp" id="onsite-xp-0" value="0" /><label for="onsite-xp-0">No</label>
                <input type="radio" name="onsite-xp" id="onsite-xp-1" value="1" /><label for="onsite-xp-1">Yes</label>
              </td>
            </tr>
            <tr>
              <td class="question-text">Did you participate in the dedicated Skype chat while volunteering in CrisisTracker?</td>
              <td>
                <input type="radio" name="in-skype-chat" id="in-skype-chat-0" value="0" /><label for="in-skype-chat-0">No</label>
                <input type="radio" name="in-skype-chat" id="in-skype-chat-1" value="1" /><label for="in-skype-chat-1">Yes</label>
              </td>
            </tr>
            <tr>
              <td class="question-text">Are you willing to participate in a 30 minute
              Skype interview about your experience as a volunteer in CrisisTracker?</td>
              <td>
                <input type="radio" name="interview" id="interview-0" value="0" /><label for="interview-0">No</label>
                <input type="radio" name="interview" id="interview-1" value="1" /><label for="interview-1">Yes</label>
                <input type="text" name="interview-comment" id="interview-comment"/><label for="interview-comment">SkypeID</label>
              </td>
            </tr>
            <tr>
              <td class="question-text">Any other thoughts you want to share with us, e.g. regarding the interface or data, or how communication and coordination was handled? (optional)</td>
              <td><textarea name="general-comment"></textarea></td>
            </tr>
          </table>
          <br />
          <div style="text-align: center; width: 90%"><input type="submit" value="Submit answers" /></div>
        </div>
      </form>
    </div>
  </div>

<?php
}
include('footer.php');
?>