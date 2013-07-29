<?php
include_once 'task_management.php';

$taskID = intval($_REQUEST['id']);
$userID = getUserID();

saveAnswer($taskID, $userID, null);

if (isset($_REQUEST["crisisid"]))
    header('Location: task_presenter.php?crisisid='.$_REQUEST["crisisid"]);
else
    header('Location: task_presenter.php');
?>
