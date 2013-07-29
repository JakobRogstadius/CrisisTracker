<?php
include "header_start.php";
printTitle("Edit Attribute");
?>
<style>
input[readonly="readonly"] {
    color: #aaa;
}
</style>
<?php
include "header_end.php";

include_once "api.php";

$maxLabels = 20;

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
function checkDelete($value) {
    if (!is_null($_POST) && array_key_exists("delete_$value", $_POST))
        return true;
    return null;
}

$attributeID = null;
$error = "";

//Did the user just submit changes to the attribute?
if (!is_null($_POST) && count($_POST) > 0) {

    $isNewAttribute = is_null($_REQUEST) || !array_key_exists("id", $_REQUEST) || $_REQUEST["id"] == '';

    $newCode = stripString1($_POST["attributeCode"]);
    $newName = stripString2($_POST["attributeName"]);
    $newDescription = stripString2($_POST["attributeDescription"]);
    if ($newCode == '')
        die("An attribute must have a code");
    if ($newName == '')
        die("An attribute must have a name");

    //Is this a new attribute?
    if ($isNewAttribute) {
        $result = createAttribute($newCode, $newName, $newDescription);
        $attributeID = intval($result[0]);
        $error = $result[1];
    } else {
        $attributeID = intval($_POST["id"]);
        if ($newCode != $_POST["oldAttributeCode"]
            || $newName != $_POST["oldAttributeName"]
            || $newDescription != $_POST["oldAttributeDescription"]) {
            $error = updateAttribute($attributeID, $newCode, $newName, $newDescription);
        }
    }

    $labelChanges = array();
    for ($i=0; $i<$maxLabels; $i++) {
        //If nothing changed, skip this label
        $code = emptyAsNull(stripString1($_POST["labelCode"][$i]));
        $nameChange = $code != null && $_POST["oldLabelName"][$i] == '' || $_POST["labelName"][$i] != $_POST["oldLabelName"][$i];
        $descChange = $_POST["labelDescription"][$i] != $_POST["oldLabelDescription"][$i];
        $delete = checkDelete($code);
        if ($code == null || !($nameChange || $descChange || $delete)) {
            continue;
        }
        $labelID = intval($_POST["labelID"][$i]);

        $change = new LabelUpdate();
        $change->id = $labelID;
        $change->attributeID = $attributeID;
        $change->code = $code;
        $change->newName = stripString2($_POST["labelName"][$i]);
        if ($change->newName == '')
            $change->newName = $change->code;
        $change->newDescription = stripString2($_POST["labelDescription"][$i]);
        $change->delete = $delete;

        $labelChanges[] = $change;
    }

    if (count($labelChanges) > 0)
        saveLabelChanges($labelChanges);
} else {
    if (!is_null($_GET) && array_key_exists("id", $_GET) && $_GET["id"] != '')
        $attributeID = intval($_GET["id"]);
}

if (strpos($error, "Duplicate entry") !== FALSE)
    $error = "Another attribute already exists with code [" . $_POST["attributeCode"] . "]. Codes must be unique.";

$attribute = null;
if (!is_null($attributeID))
    $attribute = getAttribute($attributeID);

$attributeID = is_null($attribute) ? "" : $attribute->id;
$code = is_null($attribute) ? "" : $attribute->code;
$name = is_null($attribute) ? "" : $attribute->name;
$description = is_null($attribute) ? "" : $attribute->description;

?>

<h1><a href="overview.php">Overview</a> > <a href="attributes.php">Attributes</a> > Edit Attribute</h1>
<?php if ($error != "") echo "<p class=\"error\">$error</p>" ?>
<form method="post">
    <input type="hidden" name="oldAttributeCode" value="<?php echo $code;?>" />
    <input type="hidden" name="oldAttributeName" value="<?php echo $name;?>" />
    <input type="hidden" name="oldAttributeDescription" value="<?php echo $description;?>" />
    <div class="table">
        <div class="table-row">
            <div class="table-column">Code:</div>
            <div class="table-column"><input type="text" name="attributeCode" value="<?php echo $code;?>" /></div>
            <div class="table-column">(Must be unique, can only contain [A-Z, a-z, 0-9, -, _])</div>
        </div>
        <div class="table-row">
            <div class="table-column">Name:</div>
            <div class="table-column"><input type="text" name="attributeName" value="<?php echo $name;?>" /></div>
        </div>
        <div class="table-row">
            <div class="table-column">Description:</div>
            <div class="table-column"><input type="text" name="attributeDescription" value="<?php echo $description;?>" /></div>
        </div>
        <input type="hidden" name="id" value="<?php echo $attributeID; ?>" />
    </div>

    <h2>Labels</h2>
    <p><b>Important:</b> The label code <b>null</b> should be used to represent a negative value for an attribute, e.g. <i>not an eyewitness report</i>.</p>
    <div class="table">
        <div class="table-row table-header">
            <div class="table-column">Code</div>
            <div class="table-column">Name</div>
            <div class="table-column">Description</div>
            <div class="table-column">Training data</div>
            <div class="table-column">Delete label</div>
        </div>
<?php
for ($i=0; $i<$maxLabels; $i++) {
    $label = null;
    if (!is_null($attribute) && count($attribute->labels) > $i)
        $label = $attribute->labels[$i];
    echo '<div class="table-row">';
    echo '<input type="hidden" name="oldLabelName[]" value="' . (is_null($label) ? '' : $label->name) . '" />';
    echo '<input type="hidden" name="oldLabelDescription[]" value="' . (is_null($label) ? '' : $label->description) . '" />';
    echo '<input type="hidden" name="labelID[]" value="' . (is_null($label) ? '' : $label->id) . '" ' . (is_null($label) ? '' : 'readonly="readonly"') . ' />';
    echo '<div class="table-column"><input type="text" name="labelCode[]" value="' . (is_null($label) ? '' : $label->code) . '" ' . (is_null($label) ? '' : 'readonly="readonly"') . ' /></div>';
    echo '<div class="table-column"><input type="text" name="labelName[]" value="' . (is_null($label) ? '' : $label->name) . '" /></div>';
    echo '<div class="table-column"><input type="text" name="labelDescription[]" value="' . (is_null($label) ? '' : $label->description) . '" /></div>';
    if (!is_null($label)) {
        echo '<div class="table-column">'.$label->labeledItemsCount.'</div>';
        echo '<div class="table-column"><input type="checkbox" name="delete_'.$label->code.'" /></div>';
    }
    echo '</div>';
}
?>
    </div>
    <input type="submit" value="Save changes" />
</form>

<?php
include "footer.php";
?>