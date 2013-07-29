<?php
header('Access-Control-Allow-Origin: *');

$id = intval($_REQUEST['id']);

$attributes = array();
$attribute = null;

try {
    include 'database.php';

    $sql =
        "select AttributeID, AttributeName, LabelID, LabelName, TagCount
        from StoryAidrAttributeTag
        natural join AidrLabel
        natural join AidrAttribute
        where StoryID=$id
        order by AttributeID, TagCount desc";

    $conn = get_mysql_connection();
    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
    $result = $conn->query($sql);
    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

    while($row = $result->fetch_object()) {
        if (is_null($attribute) || $attribute->attribute_id != $row->AttributeID) {
            $attribute = new stdClass;
            $attribute->attribute_id = $row->AttributeID;
            $attribute->attribute_name = $row->AttributeName;
            $attribute->labels = array();
            $attributes[] = $attribute;
        }

        $label = new stdClass;
        $label->label_id = $row->LabelID;
        $label->label_name = $row->LabelName;
        $label->tag_count = $row->TagCount;
        $attribute->labels[] = $label;
    }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($attributes);
?>