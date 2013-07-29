<?php
include "header_start.php";
printTitle("Manage Crisis Types");
include "header_end.php";
include_once "api.php";

//Did the user just submit changes?
if (!is_null($_POST) && key_exists('newName', $_POST)) {
    $name = stripString2($_POST["newName"]);
    createCrisisType($name);
}
elseif (!is_null($_GET) && key_exists('delete', $_GET)) {
    $deleteID = intval($_GET["delete"]);
    deleteCrisisType($deleteID);
}
?>

<h1><a href="overview.php">Overview</a> > Crisis Types</h1>

<form method="post" action="crisistypes.php">
    <div>
        New crisis type:
        <input type="text" name="newName" />
        <input type="submit" value="Create" />
    </div>
</form>
<br/>
<br/>
<div class="table">
    <div class="table-row table-header">
        <div class="table-column">Name</div>
        <div class="table-column">#crises</div>
        <div class="table-column">#labeled data</div>
        <div class="table-column"></div>
    </div>
<?php

$types = getCrisisTypes();
foreach ($types as $t) {
    echo '<div class="table-row">';
    echo '<div class="table-column">'.$t->name.'</div>';
    echo '<div class="table-column">'.$t->crisisCount.'</div>';
    echo '<div class="table-column">'.$t->labeledDocumentCount.'</div>';
    $alert = "Deleting [". $t->name ."] will also delete any crises of this type and all training data associated with those crises. Proceed?";
    echo '<div class="table-column"><button onclick="confirmGoto(\''.$alert.'\', \'?delete='. $t->id .'\')">Delete</button></div>';
    echo '</div>';
}
echo '</div>';

include "footer.php";
?>