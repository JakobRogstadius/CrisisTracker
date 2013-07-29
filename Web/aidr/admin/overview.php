<?php
include "header_start.php";
printTitle("Overview");
?>
<style>
    div.crisis {
        display: block;
        margin-bottom: 40px;
    }
    div.crisis .crisis-name{
        font-weight: bold;
        font-size: 1.3em;
        margin: 10px 0px 10px 0px;
    }

</style>
<?php
include "header_end.php";
include_once "api.php";

//Delete crisis?
if (!is_null($_GET) && key_exists('delete', $_GET)) {
    $deleteID = intval($_GET["delete"]);
    deleteCrisis($deleteID);
}

echo '<h1>Overview</h1>';
echo "<button onclick=\"goto('crisistypes.php')\">Manage crisis types</button>";
echo "<button onclick=\"goto('attributes.php')\">Manage attributes</button>";
echo "<button onclick=\"goto('edit_crisis.php')\">Add new crisis</button>";
echo "<br/><br/>";
$crises = getOverview();

foreach($crises as $c) {
    echo '<div class="crisis">';
    $id = $c->id;
    $code = $c->crisisCode;
    $name = $c->crisisName;
    echo "<div class=\"crisis-name\">$name [$code]";
    echo '<span style="float: right">'
        ."<button onclick=\"goto('crisis.php?id=$id')\">Details</button>"
        ."<button onclick=\"goto('edit_crisis.php?id=$id')\">Edit</button>"
        ."<button onclick=\"confirmGoto('Deleting [$code]: $name will permanently delete all its training data and prevent it from being used to improve models in future crises. Do you want to proceed?', '?delete=$id')\">Delete</button>"
        .'</div>';

    echo '<div class="table-row table-header model-family">';
    echo '<div class="table-column attribute-name">Classifier name</div>';
    echo '<div class="table-column status">Status</div>';
    echo '<div class="table-column precision">Precision</div>';
    echo '<div class="table-column recall">Recall</div>';
    echo '<div class="table-column evaluation-size">#non-null</div>';
    echo '<div class="table-column training-size">#training</div>';
    echo '<div class="table-column evaluation-size">#eval</div>';
    echo '<div class="table-column last-used">Model trained at</div>';
    echo '</div>';

    foreach($c->modelFamilies as $m) {
        echo '<div class="table-row model-family">';
        echo '<div class="table-column attribute-name">'.$m->attributeName.'</div>';
        echo '<div class="table-column status">'.$m->status.'</div>';
        echo '<div class="table-column right precision">'.$m->precision.'</div>';
        echo '<div class="table-column right recall">'.$m->recall.'</div>';
        echo '<div class="table-column right non-null-size">'.$m->nonNullCount.'</div>';
        echo '<div class="table-column right training-size">'.$m->trainingSampleCount.'</div>';
        echo '<div class="table-column right evaluation-size">'.$m->evaluationSampleCount.'</div>';
        echo '<div class="table-column last-used">'.$m->trainingTime.'</div>';
        echo '</div>';
    }
    echo '</div>';
}

include "footer.php";
?>
