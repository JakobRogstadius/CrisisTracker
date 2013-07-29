<?php
include "header_start.php";
printTitle("Overview");
?>
<style>
    h2 {
        border-bottom: 1px solid black;
        width: 800px;
        padding-top: 25px;
    }
    .disabled h2 {
        border-bottom: 1px solid #999 !important;
    }
    p {
        width: 800px;
    }
    .disabled {
        color: #999;
    }
</style>
<?php
include "header_end.php";
include_once "api.php";

if (is_null($_GET) || !key_exists('id', $_GET)) {
    die("No crisis id specified.");
}

$id = intval($_GET["id"]);

$crisis = getCrisis($id);
$perfHistory = getModelPerformanceHistory($id);
$labelDistr = getLabelDistribution($id);

echo '<h1><a href="overview.php">Overview</a> > '. $crisis->name .' ['. $crisis->code .']</h1>';
echo '<p><b>Precision:</b> The percentage of documents in the evaluation set that were assigned a non-null label and should have that label.<br/>';
echo '<b>Recall:</b> The percentage of documents in the evaluation set that should have a non-null label and were assigned that label.</p>';
echo '<p>Listed performance scores are estimates at the time of training each model. As the evaluation set grows, the estimate improves and the listed performance may decrease.</p>';

for ($i=0; $i<count($perfHistory); $i++) {
    $h = $perfHistory[$i];

    $disabled = ($h->isActive ? "" : "disabled");
    echo "<div class=\"attribute $disabled\">";
    echo '<h2>'. $h->attributeName .' ['. $h->attributeCode .']</h2>';

    //Performance history table
    echo '<div class="table">';
    echo '<div class="table-row table-header">';
    echo '<div class="table-column">#Training samples</div>';
    for ($j=0; $j<count($h->rows); $j++) {
        echo '<div class="table-column right">'. $h->rows[$j]->trainingCount .'</div>';
    }
    echo '</div>';
    echo '<div class="table-row">';
    echo '<div class="table-column">Mean precision</div>';
    for ($j=0; $j<count($h->rows); $j++) {
        echo '<div class="table-column right">'. round(100*$h->rows[$j]->avgPrecision) .'%</div>';
    }
    echo '</div>';
    echo '<div class="table-row">';
    echo '<div class="table-column">Mean recall</div>';
    for ($j=0; $j<count($h->rows); $j++) {
        echo '<div class="table-column right">'. round(100*$h->rows[$j]->avgRecall) .'%</div>';
    }
    echo '</div>';
    echo '</div>';
    echo '<br/>';

    //Training set size
    if (count($labelDistr) > 0) {
        $d = $labelDistr[$h->attributeID];

        echo '<div class="table">';
        echo '<div class="table-row table-header">';
        echo '<div class="table-column">Label</div>';
        echo '<div class="table-column">#Training<br/>samples</div>';
        echo '<div class="table-column">#Evaluation<br/>samples</div>';
        echo '<div class="table-column">#Total</div>';
        echo '</div>';
        for ($j=0; $j<count($d->rows); $j++) {
            echo '<div class="table-row">';
            echo '<div class="table-column">'.$d->rows[$j]->labelCode.'</div>';
            echo '<div class="table-column">'.$d->rows[$j]->labelCountTraining.'</div>';
            echo '<div class="table-column">'.$d->rows[$j]->labelCountEvaluation.'</div>';
            echo '<div class="table-column">'.$d->rows[$j]->labelCountTotal.'</div>';
            echo '</div>';
        }
        echo '</div>';
    }
}

include "footer.php";
?>
