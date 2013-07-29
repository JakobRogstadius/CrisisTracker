<?php
include '../db.php';
include 'data_structures.php';

function stripString1($str) {
    return preg_replace("/[^-A-Za-z0-9_\.]/i", '', $str);
}

function stripString2($str) {
    return preg_replace("/[^-A-Za-z0-9() _\/\.\,\?\!]/i", '', $str);
}

function getOverview() {
    $sql =
        "select
            ct.crisisTypeID,
            ct.name as crisisTypeName,
            c.crisisID,
            c.code as crisisCode,
            c.name as crisisName,
            m.trainingTime as trainingTime,
            mf.modelFamilyID,
            na.nominalAttributeID,
            na.code as attributeCode,
            na.name as attributeName,
            na.description as attributeDescription,
            isActive,
            case when avgPrecision is not null and isActive then 'running'
                     when avgPrecision is null and isActive then 'awaiting data'
                     when not isActive then 'disabled' end as status,
            avgPrecision,
            avgRecall,
            sum(not d.isEvaluationSet and hasHumanLabels) as trainingSetSize,
            sum(d.isEvaluationSet and hasHumanLabels) as evaluationSetSize,
            sum(hasHumanLabels and nl.nominalLabelCode != 'null') as nonNullCount
        from crisis c
        join crisis_type ct on ct.crisisTypeID=c.CrisisTypeID
        left join model_family mf on mf.crisisID=c.crisisID
        left join nominal_attribute na on na.nominalAttributeID=mf.nominalAttributeID
        left join model m on m.modelFamilyID=mf.modelFamilyID and m.isCurrentModel
        left join nominal_label nl on nl.nominalAttributeID=mf.nominalAttributeID
        left join document_nominal_label dnl on dnl.nominalLabelID=nl.nominalLabelID
        left join document d on d.documentID=dnl.documentID and d.crisisID=c.crisisID
        group by crisisID, modelFamilyID
        order by c.crisisID, isActive desc, m.modelID is null, avgPrecision desc";

    $crises = array();
    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        $c = null;
        while ($row = $result->fetch_assoc()) {
            $crisisID = intval($row["crisisID"]);
            if (is_null($c) || $c->id != $crisisID) {
                $c = new CrisisSummary();
                $c->id = $crisisID;
                $c->crisisName = $row["crisisName"];
                $c->crisisCode = $row["crisisCode"];
                $c->crisisTypeName = $row["crisisTypeName"];
                $c->modelFamilies = array();
                $crises[] = $c;
            }

            $m = new ModelFamilySummary();
            $m->id = intval($row["modelFamilyID"]);
            $m->attributeID = $row["nominalAttributeID"];
            $m->attributeCode = $row["attributeCode"];
            $m->attributeName = $row["attributeName"];
            $m->attributeDescription = $row["attributeDescription"];
            $m->status = $row["status"];
            $m->trainingTime = $row["trainingTime"];
            $m->isActive = $row["isActive"];
            $m->precision = round(100 * floatval($row["avgPrecision"])) . '%';
            $m->recall = round(100 * floatval($row["avgRecall"])) . '%';
            $m->trainingSampleCount = $row["trainingSetSize"];
            $m->evaluationSampleCount = $row["evaluationSetSize"];
            $m->nonNullCount = $row["nonNullCount"];
            $c->modelFamilies[] = $m;
        }
        $result->close();

    }
    $con->close();

    return $crises;
}

function getAttributes() {
    $sql =
        "select
            na.nominalAttributeID,
            na.code,
            na.name,
            na.description,
            group_concat(nominalLabelCode order by nominalLabelCode!='null', nominalLabelCode separator ', ') as labels,
            (
                select count(*) 
                from document_nominal_label dnl
                join nominal_label lbl on lbl.nominalLabelID=dnl.nominalLabelID
                where lbl.nominalAttributeID=na.nominalAttributeID
            ) as labelCount
        from nominal_attribute na
        left join nominal_label nl on nl.nominalAttributeID=na.nominalAttributeID
        group by na.nominalAttributeID;";

    $attributes = array();
    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        while ($row = $result->fetch_assoc()) {
            $a = new NominalAttribute();
            $a->id = $row["nominalAttributeID"];
            $a->code = $row["code"];
            $a->name = $row["name"];
            $a->description = $row["description"];
            $a->labels = $row["labels"];
            $a->labeledItemsCount = intval($row["labelCount"]);
            $attributes[] = $a;
        }
        $result->close();
    }

    $con->close();
    return $attributes;
}

function getAttribute($attributeID) {
    $attributeID = intval($attributeID);
    $sql =
        "select
            na.nominalAttributeID,
            na.code as attributeCode,
            na.name as attributeName,
            na.description as attributeDescription,
            nl.nominalLabelID,
            nl.nominalLabelCode,
            nl.name as labelName,
            nl.description as labelDescription,
            count(distinct dnl.documentID) as labelCount
    from nominal_attribute na
        left join nominal_label nl on nl.nominalAttributeID=na.nominalAttributeID
        left join document_nominal_label dnl on dnl.nominalLabelID=nl.nominalLabelID
        where na.nominalAttributeID = $attributeID
        group by na.nominalAttributeID, nl.nominalLabelID
        order by nl.nominalLabelCode!='null', nl.nominalLabelCode";

    $attribute = new NominalAttribute();
    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        while ($row = $result->fetch_assoc()) {
            if (is_null($attribute->id)) {
                $attribute->id = intval($row["nominalAttributeID"]);
                $attribute->code = $row["attributeCode"];
                $attribute->name = $row["attributeName"];
                $attribute->description = $row["attributeDescription"];
                $attribute->labels = array();
            }
            $l = new NominalAttributeLabel();
            $l->id = $row["nominalLabelID"];
            $l->code = $row["nominalLabelCode"];
            $l->name = $row["labelName"];
            $l->description = $row["labelDescription"];
            $l->labeledItemsCount = intval($row["labelCount"]);
            if ($l->code != null)
                $attribute->labels[] = $l;
        }
        $result->close();
    }

    $con->close();
    return $attribute;
}

function deleteAttribute($attributeID) {
    $attributeID = intval($attributeID);
    $con = getMySqlConnection();
    $con->query("delete from nominal_attribute where nominalAttributeID=$attributeID limit 1");
    $con->close();
}

function createAttribute($code, $name, $description) {
    $code = stripString1($code);
    $name = stripString2($name);
    $description = stripString2($description);

    $con = getMySqlConnection();
    $con->query("insert into nominal_attribute (code, name, description) values ('$code','$name','$description')");
    $id = $con->insert_id;
    $error = $con->error;
    $con->close();

    return array($id, $error);
}

function updateAttribute($attributeID, $code, $name, $description) {
    $attributeID = intval($attributeID);
    $code = stripString1($code);
    $name = stripString2($name);
    $description = stripString2($description);

    $con = getMySqlConnection();
    $con->query(
        "update nominal_attribute set code='$code', name='$name', description='$description'
        where nominalAttributeID=$attributeID limit 1");
    $error = $con->error;
    $con->close();
    return $error;
}

function saveLabelChanges($changes) {
    $con = getMySqlConnection();

    foreach ($changes as $change) {
        if ($change->delete) {
            $id = intval($change->id);
            $con->query("delete from nominal_label where nominalLabelID=$id limit 1");
        }
        else {
            $code = $change->code;
            $attributeID = $change->attributeID;
            $name = $change->newName;
            $desc = $change->newDescription;
            $sql =
            $con->query(
                "insert into nominal_label (nominalAttributeID, nominalLabelCode, name, description)
                values ($attributeID, '$code', '$name', '$desc')
                on duplicate key update name=values(name), description=values(description)");
        }
    }

    $con->close();
}

function getAttributesForCrisis($crisisID, $crisisTypeID) {
    $crisisID = intval($crisisID);
    $crisisTypeID = intval($crisisTypeID);

    $sql =
        "select
            na.nominalAttributeID,
            na.code,
            na.name,
            na.description,
            group_concat(distinct nl.nominalLabelCode order by nl.nominalLabelCode!='null', nl.nominalLabelCode separator ', ') as labels,
            count(distinct d.documentID) as totalLabels,
            sum(c.crisisTypeID=$crisisTypeID) as crisisTypeLabels,
            sum(c.crisisID=$crisisID) as crisisLabels
        from nominal_attribute na
        left join nominal_label nl on nl.nominalAttributeID=na.nominalAttributeID
        left join document_nominal_label dnl on dnl.nominalLabelID=nl.nominalLabelID
        left join document d on d.documentID=dnl.documentID
        left join crisis c on c.crisisID=d.crisisID
        group by na.nominalAttributeID
        order by crisisLabels desc, crisisTypeLabels desc, nominalAttributeID";

    $attributes = array();
    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        while ($row = $result->fetch_assoc()) {
            $a = new NominalAttribute();
            $a->id = $row["nominalAttributeID"];
            $a->code = $row["code"];
            $a->name = $row["name"];
            $a->description = $row["description"];
            $a->labels = $row["labels"];
            $a->labeledItemsCount = intval($row["totalLabels"]);
            $a->labeledItemsCountCrisis = intval($row["crisisLabels"]);
            $a->labeledItemsCountCrisisType = intval($row["crisisTypeLabels"]);
            $attributes[] = $a;
        }
        $result->close();
    }

    $con->close();
    return $attributes;
}

function getCrisis($crisisID) {
    $sql =
        "select c.crisisID, c.code, c.name, c.crisisTypeID, mf.nominalAttributeID
        from crisis c
        left join model_family mf on mf.crisisID=c.crisisID and mf.isActive
        where c.crisisID=$crisisID
        order by nominalAttributeID";

    $crisis = new Crisis();
    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        while ($row = $result->fetch_assoc()) {
            if (is_null($crisis->id)) {
                $crisis->id = intval($row["nominalAttributeID"]);
                $crisis->code = $row["code"];
                $crisis->name = $row["name"];
                $crisis->eventTypeID = $row["crisisTypeID"];
                $crisis->activeAttributeIDs = array();
            }
            if (!is_null($row["nominalAttributeID"])) {
                $crisis->activeAttributeIDs[] = intval($row["nominalAttributeID"]);
            }
        }
        $result->close();
    }

    $con->close();
    return $crisis;
}

function getCrisisTypes() {
    $sql =
        "select
            ct.crisisTypeID,
            ct.name,
            count(distinct c.crisisID) as crisisCount,
            count(distinct d.documentID) as labeledDocumentCount
        from crisis_type ct
        left join crisis c on c.crisisTypeID=ct.crisisTypeID
        left join document d on d.crisisID=c.crisisID and d.hasHumanLabels
        group by crisisTypeiD";

    $types = array();

    $con = getMySqlConnection();
    if ($result = $con->query($sql)) {
        while ($row = $result->fetch_assoc()) {
            $t = new CrisisType();
            $t->id = $row["crisisTypeID"];
            $t->name = $row["name"];
            $t->crisisCount = intval($row["crisisCount"]);
            $t->labeledDocumentCount = intval($row["labeledDocumentCount"]);
            $types[] = $t;
        }
        $result->close();
    }

    $con->close();
    return $types;
}
function createCrisisType($name) {
    $con = getMySqlConnection();
    $con->query("insert into crisis_type (name) values ('$name')");
    $con->close();
}
function deleteCrisisType($id) {
    $id = intval($id);
    $con = getMySqlConnection();
    $con->query("delete from crisis_type where crisisTypeID=$id limit 1");
    $con->close();
}
function createCrisis($code, $name, $crisisTypeID) {
    $code = stripString1($code);
    $name = stripString2($name);
    $crisisTypeID = intval($crisisTypeID);

    $con = getMySqlConnection();
    $con->query("insert into crisis (code, name, crisisTypeID) values ('$code','$name',$crisisTypeID)");
    $id = $con->insert_id;
    $error = $con->error;
    $con->close();

    return array($id, $error);
}

function updateCrisis($crisisID, $code, $name, $crisisTypeID) {
    $crisisID = intval($crisisID);
    $code = stripString1($code);
    $name = stripString2($name);
    $crisisTypeID = intval($crisisTypeID);

    $con = getMySqlConnection();
    $con->query(
        "update crisis set code='$code', name='$name', crisisTypeID=$crisisTypeID
        where crisisID=$crisisID limit 1");
    $error = $con->error;
    $con->close();
    return $error;
}

function updateCrisisAttributes($crisisID, $removedIDs, $addedIDs) {
    $crisisID = intval($crisisID);
    $con = getMySqlConnection();
    foreach ($removedIDs as $attributeID) {
        if (!is_int($attributeID))
            continue;
        $con->query(
            "update model_family set isActive=0
            where crisisID=$crisisID and nominalAttributeID=$attributeID");
        $con->query(
            "delete mf.* from model_family mf 
                left join nominal_label lbl on lbl.nominalAttributeID = mf.nominalAttributeID
                left join document_nominal_label dnl on dnl.`nominalLabelID` = lbl.`nominalLabelID`
                left join document d on d.documentID = dnl.`documentID` AND d.crisisID = mf.crisisID
                where dnl.nominalLabelID is NULL and mf.isActive = 0  and mf.crisisID=$crisisID;");
    }
    foreach ($addedIDs as $attributeID) {
        if (!is_int($attributeID))
            continue;
        $con->query(
            "insert into model_family (crisisID, nominalAttributeID)
            values ($crisisID, $attributeID) on duplicate key update isActive=1");
    }
    $con->close();
}


function deleteCrisis($crisisID) {
    $crisisID = intval($crisisID);
    $con = getMySqlConnection();
    $con->query("delete from crisis where crisisID=$crisisID limit 1");
    $con->close();
}

function getModelPerformanceHistory($crisisID) {
    $crisisID = intval($crisisID);
    $sql =
        "select
                mf.nominalAttributeID,
                a.code as attributeCode,
                a.name as attributeName,
                m.isCurrentModel,
                avgPrecision,
                avgRecall,
                trainingCount,
                trainingTime,
                isActive
        from model_family mf
        left join model m on m.modelFamilyID = mf.modelFamilyID
        left join nominal_attribute a on a.nominalAttributeID=mf.nominalAttributeID
        where crisisID=$crisisID
        order by isActive desc, mf.nominalAttributeID, m.modelID";
    $attributes = array();
    $con = getMySqlConnection();
    $result = $con->query($sql);
    if ($result) {
        $a = null;
        while ($row = $result->fetch_assoc()) {
            $attributeID = intval($row["nominalAttributeID"]);
            if (is_null($a) || $a->attributeID != $attributeID) {
                $a = new AttributeStatistics();
                $a->attributeID = $attributeID;
                $a->attributeCode = $row["attributeCode"];
                $a->attributeName = $row["attributeName"];
                $a->isActive = $row["isActive"];
                $a->rows = array();
                $attributes[] = $a;
            }

            $h = new ModelPerformanceHistory();
            $h->isActiveModel = $row["isCurrentModel"];
            $h->avgPrecision = floatval($row["avgPrecision"]);
            $h->avgRecall = floatval($row["avgRecall"]);
            $h->trainingCount = intval($row["trainingCount"]);
            $h->trainingTime = $row["trainingTime"];
            $a->rows[] = $h;
        }
    }
    return $attributes;
}

function getLabelDistribution($crisisID) {
    $crisisID = intval($crisisID);
    $sql =
        "select
            mf.nominalAttributeID,
            nl.nominalLabelCode,
            count(distinct d.documentID) as labelCountTotal,
            sum(!isEvaluationSet) as labelCountTraining,
            sum(isEvaluationSet) as labelCountEvaluation
        from model_family mf
        left join nominal_label nl on nl.nominalAttributeID=mf.nominalAttributeID
        left join document_nominal_label dnl on dnl.nominalLabelID=nl.nominalLabelID
        left join document d on d.documentID=dnl.documentID
        where mf.crisisID=$crisisID
        group by nominalAttributeID, nominalLabelCode
        order by nominalAttributeID, nominalLabelCode!='null', labelCountTraining desc";
    $attributes = array();
    $con = getMySqlConnection();
    $result = $con->query($sql);
    if ($result) {
        $a = null;
        while ($row = $result->fetch_assoc()) {
            $attributeID = intval($row["nominalAttributeID"]);
            if (is_null($a) || $a->attributeID != $attributeID) {
                $a = new AttributeStatistics();
                $a->attributeID = $attributeID;
                $a->rows = array();
                $attributes[$attributeID] = $a;
            }

            $d = new LabelDistribution();
            $d->labelCode = $row["nominalLabelCode"];
            $d->labelCountTotal = intval($row["labelCountTotal"]);
            $d->labelCountTraining = floatval($row["labelCountTraining"]);
            $d->labelCountEvaluation = intval($row["labelCountEvaluation"]);
            $a->rows[] = $d;
        }
    }
    return $attributes;
}
?>