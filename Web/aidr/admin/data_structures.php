<?php
class CrisisSummary {
    public $id;
    public $crisisCode;
    public $crisisName;
    public $crisisTypeName;
    public $modelFamilies;
}

class Crisis {
    public $id;
    public $code;
    public $name;
    public $eventTypeID;
    public $activeAttributeIDs;
}

class CrisisType {
    public $id;
    public $name;
    public $crisisCount;
    public $labeledDocumentCount;
}

class NominalAttribute {
    public $id;
    public $code;
    public $name;
    public $description;
    public $labels;
    public $labeledItemsCount;
    public $labeledItemsCountCrisis;
    public $labeledItemsCountCrisisType;
}

class NominalAttributeLabel {
    public $code;
    public $name;
    public $description;
    public $labeledItemsCount;
}

class LabelUpdate {
    public $attributeID;
    public $code;
    public $newName;
    public $newDescription;
    public $delete = 0;
}

class ModelFamilySummary {
    public $id;
    public $attributeID;
    public $attributeCode;
    public $attributeName;
    public $attributeDescription;
    public $status;
    public $isActive;
    public $precision;
    public $recall;
    public $lastInputTime;
    public $trainingSampleCount;
    public $evaluationSampleCount;
    public $nonNullCount;
}

class AttributeStatistics {
    public $attributeID;
    public $attributeCode;
    public $attributeName;
    public $rows;
}

class ModelPerformanceHistory {
    public $isActiveModel;
    public $avgPrecision;
    public $avgRecall;
    public $trainingCount;
    public $trainingTime;
}

class LabelDistribution {
    public $labelCode;
    public $labelCountTotal;
    public $labelCountTraining;
    public $labelCountEvaluation;
}

?>
