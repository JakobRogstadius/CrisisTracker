<?php
include_once '../db.php';
include_once 'task.php';
include_once 'nominal_label.php';
require '../Predis/Autoloader.php';
Predis\Autoloader::register();

$answersPerTask = 1;
$minAgreement = 1;
$maxAssignmentDuration = 5;
$dontknow = 4294967295;

function getUserID() {
    return ip2long($_SERVER['REMOTE_ADDR']);
}

function getCrisisID($crisisCode) {
    $con = getMySqlConnection();
    $crisisCode = $con->real_escape_string($crisisCode);
    $crisisID = intval(selectSingleValue($con, "select crisisID from crisis where code = '$crisisCode'"));
    $con->close();

    return $crisisID;
}

function getTask($crisisID, $userID) {
    global $maxAssignmentDuration;
    global $answersPerTask;
    $userID = intval($userID);
    $con = getMySqlConnection();

    $crisisWhere = "";
    if (!is_null($crisisID))
        $crisisWhere = " and b.crisisID=" . intval($crisisID);

    //Delete expired assignments
    $con->query("delete from task_assignment where assignedAt < utc_timestamp()-interval $maxAssignmentDuration minute limit 100");

    $task = null;
    $con->query("SET group_concat_max_len = 10240;");
    $getTaskSql = "select b.* from task_buffer b
        left join task_answer ans on ans.documentID=b.documentID and ans.userID=$userID
        left join task_assignment asg on asg.documentID=b.documentID and asg.userID=$userID
        where ans.documentID is null
            and (b.assignedCount < $answersPerTask or asg.documentID is not null)
            $crisisWhere
        order by asg.documentID is not null desc, valueAsTrainingSample desc, b.documentID desc
        limit 1";
    if ($result = $con->query($getTaskSql)) {
        if ($row = $result->fetch_assoc()) {

            //Get a task
            $task = new Task(
                $row["documentID"],
                $row["crisisID"],
                $row["attributeInfo"],
                $row["language"],
                $row["doctype"],
                $row["data"]);

            //Mark the task as assigned to this user with timestamp
            $success = $con->query("insert into task_assignment (documentID, userID, assignedAt) values ("
                . intval($row["documentID"]) . ",$userID,utc_timestamp())"
                . " on duplicate key update assignedAt=values(assignedAt)");
        }
        $result->close();
    }

    $con->close();

    return $task;
}

function returnTask($taskID, $userID) {
    $taskID = intval($taskID);
    $userID = intval($userID);
    $con = getMySqlConnection();
    $con->query("delete from task_assignment where documentID=$taskID and userID=$userID");
    $con->close();
}

function saveAnswer($taskID, $userID, $labels) {
    global $answersPerTask;
    $con = getMySqlConnection();

    //Save this answer
    $taskID = intval($taskID);
    $userID = intval($userID);
    $json = json_encode($labels);
    $con->query("insert ignore into task_answer (documentID, userID, answer, timestamp) values ($taskID, $userID, '$json', utc_timestamp())");

    //If enough answers are available for this task, aggregate them
    //A 'null' answer implies that the user skipped the task
    $answerCount = intval(selectSingleValue($con, "select count(*) as AnswerCount from task_answer where documentID=$taskID and answer != 'null'"));
    if ($answerCount >= $answersPerTask)
        aggregateTaskAnswers($taskID);

    $con->close();

    returnTask($taskID, $userID);
}

function selectSingleValue($con, $sql) {
    $result = $con->query($sql);
    $row = $result->fetch_assoc();
    return reset($row);
}

function aggregateTaskAnswers($taskID) {
    global $minAgreement;
    global $dontknow;
    $taskID = intval($taskID);
    $con = getMySqlConnection();

    //Get all answers for this task
    if ($result = $con->query("select answer from task_answer where documentID = $taskID and answer!='null'")) {

        //Find the most frequent label per attribute
        $answers = array();
        while ($row = $result->fetch_assoc()) {
            $answerSet = json_decode($row["answer"]);
            foreach($answerSet as $answer) {
                $answers[$answer->attributeID][] = $answer->labelID;
            }
        }
        $result->close();

        $finalAnswers = array();
        foreach($answers as $attributeID => $labelIDs) {
            $c = array_count_values($labelIDs);
            if (max($c) < $minAgreement) {
                //More answers are needed to reach agreement. Clean up and return.
                $con->close();
                return;
            }
            $val = array_search(max($c), $c);
            $finalAnswers[$attributeID] = intval($val);
        }

        //Save the final answers
        foreach($finalAnswers as $attributeID => $label) {
            if ($label == $dontknow)
                continue;
            $con->query("insert ignore into document_nominal_label (documentID, nominalLabelID, timestamp) values "
                         . "($taskID, $label, utc_timestamp())");
        }

        //Mark task as completed
        $con->query("update document set hasHumanLabels=1 where documentID=$taskID");

        //Send notification that a new training sample has arrived
        $crisisID = intval(selectSingleValue($con, "select crisisid from document where documentid=$taskID"));
        notifyRedis($crisisID, array_keys($answers));
    }

    $con->close();
}

function notifyRedis($crisisID, $attributeIDs) {
    $json = "{crisis_id:$crisisID, attributes:[" . implode(',',$attributeIDs) . "]}";

    $redis = new Predis\Client('tcp://127.0.0.1:6379');
    $redis->rpush("training_sample_info_stream", $json);
}


function getTodayTaskCount($userID, $crisisID) {

}
?>
