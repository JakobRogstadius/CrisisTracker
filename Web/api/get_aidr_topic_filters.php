<?php
header('Access-Control-Allow-Origin: *');

$attributes = array();
$attribute = null;

try {
    include 'database.php';

    $sql = "select
            a.AttributeID,
            a.AttributeName,
            a.AttributeDescription,
            l.LabelID,
            l.LabelName,
            l.LabelDescription,
            count(distinct st.StoryID) as StoryCount
        from AidrAttribute a
        join AidrLabel l on l.AttributeID = a.AttributeID
        left join StoryAidrAttributeTag st on st.LabelID=l.LabelID
        group by a.AttributeID, l.LabelID
        order by a.AttributeID, StoryCount desc";

    $conn = get_mysql_connection();
    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
    $result = $conn->query($sql);
    $conn->query("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ");

    while($row = $result->fetch_object()) {

        if (is_null($attribute) || $attribute->attribute_id != $row->AttributeID) {
            $attribute = new stdClass;
            $attribute->attribute_id = $row->AttributeID;
            $attribute->attribute_name = $row->AttributeName;
            $attribute->attribute_description = $row->AttributeDescription;
            $attribute->labels = array();
            $attributes[] = $attribute;
        }

        $label = new stdClass;
        $label->label_id = $row->LabelID;
        $label->label_name = $row->LabelName;
        $label->label_description = $row->LabelDescription;
        $label->story_count = $row->StoryCount;
        $attribute->labels[] = $label;
    }
} catch (Exception $e) {
  echo $e->getMessage();
}

$conn->close();

echo json_encode($attributes);
?>