<?php
function linkify($text) {
    $text = preg_replace('/http\S+/', '<a href="${0}" target="_blank">${0}</a>', $text);
    return preg_replace('/@[A-Za-z0-9_]+/', '<a href="https://twitter.com/${0}" target="_blank">${0}</a>', $text);
}

include_once "task_management.php";

$crisisID = null;
if (isset($_GET["crisiscode"])) {
    $crisisID = getCrisisID($_GET["crisiscode"]);
    header("Location: task_presenter.php?crisisid=$crisisID");
}
elseif (isset($_GET["crisisid"])) {
    $crisisID = intval($_GET["crisisid"]);
}
?>
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <script type="text/javascript" src="../jquery-1.9.1.min.js"></script>
        <script>
            function validateInput() {
                if ($("#options").find("input:checked").size() === $("#options").find("ul").size()) {
                    return true;
                } else {
                    alert("Please select one option for each label category");
                    return false;
                }
            }
            function goto(url) {
                window.location=url;
            }
            function confirmGoto(question, url) {
                if (confirm(question)) {
                    goto(url);
                }
            }
        </script>
        <link rel="stylesheet" type="text/css" href="presenter.css" media="screen" />
    </head>
    <body>
        <div id="container">
            <h1>Train the classifiers</h1>
<?php

$crisisFilterString = "";
if (isset($crisisID))
    $crisisFilterString = "?crisisid=" . $crisisID;
echo '<form method="post" action="submit_task.php' . $crisisFilterString . '" onsubmit="return validateInput()">';

$userID = getUserID();
$crisisID = null;
if (isset($_GET["crisisid"]))
    $crisisID = intval($_GET["crisisid"]);

$task = getTask($crisisID, $userID);

if (is_null($task)) {
    echo '<div class="panel"><h2>Thank you</h2><div class="subpanel">There are no more documents to label. Please check back later.</div></div>';
} else {
    echo '<h2>Document</h2>';
    echo '<div id="document" class="panel text">';
    echo '<input type="hidden" name="crisisID" value="'.$crisisID.'" />';
    echo '<input type="hidden" name="documentID" value="'.$task->documentID.'" />';
    echo "<div class=\"subpanel\">" . linkify($task->getText()) . "</div>";
    echo '</div>';

    echo '<h2>Labels</h2>';
    echo '<div id="options" class="panel">';
    foreach($task->attributeInfo as $attribute) {
        $attributeID = $attribute->{'id'};
        $attributeName = $attribute->{'name'};
        $labels = $attribute->{'labels'};
        echo '<div class="subpanel label-list">';
        echo "<h3>$attributeName</h3>";
        echo '<div><ul>';
        $dontknowID = $attributeID . "_" . $dontknow;
        foreach($labels as $label) {
            $id = $attributeID . '_' . $label->{'id'};
            echo '<li><input type="radio" name="attribute_' . $attributeID . '" value="' . $label->{'id'} . '" id="' . $id . '" />';
            echo '<label for="' . $id . '" title="' . $label->{'description'} . '">' . $label->{'name'} . '</label></li>';
        }
        echo '<li><input type="radio" name="attribute_' . $attributeID . '" checked="checked" value="' . $dontknow . '" id="' . $dontknowID . '" />';
        echo '<label for="' . $dontknowID . '">I don\'t know</label></li>';
        echo '</ul></div>';
        echo '<div class="attribute-description">' . $attribute->{'description'} . '</div>';
        echo '</div>';
    }
    echo '</div>';
}
echo '<input type="submit" value="Save labels"/> ';
if ($crisisFilterString == "")
    echo '<input type="button" value="Skip this task" onclick="goto(\'skip_task.php?id='. $task->documentID .'\')" />';
else
    echo '<input type="button" value="Skip this task" onclick="goto(\'skip_task.php'. $crisisFilterString .'&id='. $task->documentID .'\')" />';
echo '</form>';
?>
        </div>
        <h2>Instructions</h2>
        <div class="panel text">
            <p>Teach the system how to label incoming documents by labeling examples. </p>
        </div>
    </body>
</html>