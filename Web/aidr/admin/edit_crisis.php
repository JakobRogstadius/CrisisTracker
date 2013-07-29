<?php
include "header_start.php";
printTitle("Edit Crisis");
?>
<style>
input[readonly="readonly"] {
    color: #aaa;
}
</style>
<?php
include "header_end.php";

include_once "api.php";

function isNullOrEmpty($value) {
    return is_null($value) || $value == '';
}
function emptyAsNull($value) {
    if ($value=='')
        return null;
    return $value;
}
function getValueChange($oldValue, $newValue) {
    if (emptyAsNull($newValue) != $oldValue)
        return $newValue;
    else
        return null;
}

//Did the user just submit changes to the crisis?
$id = null;
$error = "";
if (!is_null($_POST) && count($_POST) > 0) {
    $newCode = stripString2($_POST["crisisCode"]);
    if ($newCode == '')
        die("A crisis must have a (unique) code");

    $newName = stripString2($_POST["crisisName"]);
    if ($newName == '')
        die("A crisis must have a name");

    $newTypeID = intval($_POST["crisisType"]);

    //Create or update the crisis itself
    $isNewCrisis = is_null($_REQUEST) || !array_key_exists("id", $_REQUEST) || $_REQUEST["id"] == '';
    if ($isNewCrisis) {
        $result = createCrisis($newCode, $newName, $newTypeID);
        $id = $result[0];
        $error = $result[1];
    }
    else {
        $id = intval($_REQUEST["id"]);
        if ($newCode != $_POST["oldCrisisCode"]
            || $newName != $_POST["oldCrisisName"]
            || $newTypeID != intval($_POST["oldCrisisTypeID"])) {
            $error = updateCrisis($id, $newCode, $newName, $newTypeID);
        }
    }

    //Add and remove attributes
    $addedAttributes = array();
    $removedAttributes = array();
    foreach ($_POST as $key => $value) {
        if (substr($key, 0, 7) == "remove_")
            $removedAttributes[] = intval(substr($key, 7));
        if (substr($key, 0, 4) == "add_")
            $addedAttributes[] = intval(substr($key, 4));
    }
    if (count($addedAttributes) + count($removedAttributes) > 0) {
        updateCrisisAttributes($id, $removedAttributes, $addedAttributes);
    }

} else {
    if (!is_null($_GET) && array_key_exists("id", $_GET) && $_GET["id"] != '')
        $id = intval($_GET["id"]);
}

$crisis = null;
$crisisTypeID = null;
if (!is_null($id)) {
    $crisis = getCrisis($id);
    $crisisTypeID = $crisis->eventTypeID;
}
$code = is_null($crisis) ? "" : $crisis->code;
$name = is_null($crisis) ? "" : $crisis->name;
$crisisTypes = getCrisisTypes();
$attributes = getAttributesForCrisis($id, $crisisTypeID);

?>

<h1><a href="overview.php">Overview</a> > Edit Crisis</h1>

<form method="post">
    <input type="hidden" name="id" value="<?php echo $id; ?>" />
    <input type="hidden" name="oldCrisisCode" value="<?php echo $code;?>" />
    <input type="hidden" name="oldCrisisName" value="<?php echo $name;?>" />
    <input type="hidden" name="oldCrisisTypeID" value="<?php echo $crisisTypeID; ?>" />
    <div class="table">
        <div class="table-row">
            <div class="table-column">Code:</div>
            <div class="table-column"><input type="text" name="crisisCode" value="<?php echo $code;?>" /></div>
            <div class="table-column">(Must be unique, can only contain [A-Z, a-z, 0-9, -, _])</div>
        </div>
        <div class="table-row">
            <div class="table-column">Name:</div>
            <div class="table-column"><input type="text" name="crisisName" value="<?php echo $name;?>" /></div>
        </div>
        <div class="table-row">
            <div class="table-column">Crisis type:</div>
            <div class="table-column">
                <select name="crisisType">
<?php
foreach ($crisisTypes as $type) {
    $selected = "";
    if ($type->id==$crisisTypeID)
        $selected = 'selected="selected"';
    echo '<option value="'.$type->id.'" '.$selected.'>'.$type->name.'</option>';
}
?>
                </select>
            </div>
        </div>
    </div>

    <h2>Selected attributes</h2>
    <div class="table">
        <div class="table-row table-header">
            <div class="table-column center" style="width: 50px;">Remove</div>
            <div class="table-column">Name</div>
            <div class="table-column">Labels</div>
            <div class="table-column right">#labeled<br/>(this event)</div>
            <div class="table-column right">#labeled<br/>(similar events)</div>
            <div class="table-column">Description</div>
        </div>
<?php
foreach ($attributes as $a) {
    if (!is_null($crisis) && in_array($a->id, $crisis->activeAttributeIDs)) {
        echo '<div class="table-row">';
        echo '<div class="table-column center"><input type="checkbox" name="remove_'.$a->id.'" /></div>';
        echo '<div class="table-column">'.$a->name.'</div>';
        echo '<div class="table-column">'.$a->labels.'</div>';
        echo '<div class="table-column right">'.$a->labeledItemsCountCrisis.'</div>';
        echo '<div class="table-column right">'.$a->labeledItemsCountCrisisType.'</div>';
        echo '<div class="table-column">'.$a->description.'</div>';
        echo '</div>';
    }
}
echo '</div>';
if (is_null($crisis) || is_null($crisis->activeAttributeIDs) || count($crisis->activeAttributeIDs)==0)
    echo '<p>No attributes have been selected for this crisis</p>';
?>

    <h2>Available attributes</h2>
    <div class="table">
        <div class="table-row table-header">
            <div class="table-column center" style="width: 50px;">Add</div>
            <div class="table-column">Name</div>
            <div class="table-column">Labels</div>
            <div class="table-column right">#labeled<br/>(this event)</div>
            <div class="table-column right">#labeled<br/>(similar events)</div>
            <div class="table-column right">#labeled<br/>(all events)</div>
            <div class="table-column">Description</div>
        </div>
<?php
foreach ($attributes as $a) {
    if (is_null($crisis) || !in_array($a->id, $crisis->activeAttributeIDs)) {
        echo '<div class="table-row">';
        echo '<div class="table-column center"><input type="checkbox" name="add_'.$a->id.'" /></div>';
        echo '<div class="table-column">'.$a->name.'</div>';
        echo '<div class="table-column">'.$a->labels.'</div>';
        echo '<div class="table-column right">'.$a->labeledItemsCountCrisis.'</div>';
        echo '<div class="table-column right">'.$a->labeledItemsCountCrisisType.'</div>';
        echo '<div class="table-column right">'.$a->labeledItemsCount.'</div>';
        echo '<div class="table-column">'.$a->description.'</div>';
        echo '</div>';
    }
}
echo '</div>';
if (!is_null($crisis) && count($attributes)==count($crisis->activeAttributeIDs))
    echo '<p>There are no more attributes to add.</p>';
?>

    <input type="submit" value="Save changes" />
</form>

<?php
include "footer.php";
?>