<?php
include "header_start.php";
printTitle("Manage Attributes");
include "header_end.php";

include_once "api.php";

//Delete attribute?
if (!is_null($_GET) && key_exists('delete', $_GET)) {
    $deleteID = intval($_GET["delete"]);
    deleteAttribute($deleteID);
}
?>

<h1><a href="overview.php">Overview</a> > Attributes</h1>
<input type="button" value="Add new attribute" onclick="goto('edit_attribute.php')" />

<br/><br/>

<div class="table-row table-header">
    <div class="table-column right">ID</div>
    <div class="table-column">Code</div>
    <div class="table-column">Name</div>
    <div class="table-column">Labels</div>
    <div class="table-column">Description</div>
    <div class="table-column right">Training samples</div>
    <div class="table-column"> </div>
</div>
<?php
$attributes = getAttributes();
foreach ($attributes as $a) {
    $id = $a->id;
    $name = $a->name;
    echo '<div class="table-row">';
    echo '<div class="table-column right">'.$a->id.'</div>';
    echo '<div class="table-column">'.$a->code.'</div>';
    echo '<div class="table-column">'.$a->name.'</div>';
    echo '<div class="table-column">'.$a->labels.'</div>';
    echo '<div class="table-column">'.$a->description.'</div>';
    echo '<div class="table-column right">'.$a->labeledItemsCount.'</div>';
    echo '<div class="table-column" style="width:100px">';
    echo '<button onclick="goto(\'edit_attribute.php?id='.$a->id.'\')">Edit</button>';
    echo '<button onclick="confirmGoto(\'Deleting ['.$name.'] will permanently remove it from all crises and delete all its training data, which prevents it from being used to improve models in future crises. Do you want to proceed?\', \'?delete='.$id.'\')">Delete</button>';
    echo '</div>';
    echo '</div>';
}

include "footer.php";
?>