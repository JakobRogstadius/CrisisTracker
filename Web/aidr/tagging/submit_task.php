<?php
include_once 'task_management.php';
include_once 'nominal_label.php';

//Get answers
$userID = getUserID();
$taskID = $_POST['documentID'];

$labels = array();
foreach($_POST as $key => $val) {
  if(strpos($key, 'attribute_') === 0 ) {
    $label = new NominalLabel();
    $label->attributeID = intval(substr($key, 10));
    $label->labelID = intval($val);
    $labels[] = $label;
  }
}

//Save results
saveAnswer($taskID, $userID, $labels);

if (isset($_REQUEST["crisisid"]))
    header('Location: task_presenter.php?crisisid='.$_REQUEST["crisisid"]);
else
    header('Location: task_presenter.php');
?>
