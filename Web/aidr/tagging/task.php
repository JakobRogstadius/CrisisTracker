<?php

class Task {
  public $documentID;
  public $crisisID;
  public $attributeInfo;
  public $language;
  public $doctype;
  public $document;

  public function __construct($docID, $crsID, $attrInfo, $lang, $doctype, $docdata) {
    $this->documentID = intval($docID);
    $this->crisisID = intval($crsID);
    $this->attributeInfo = json_decode($attrInfo)->{'attributes'};
    $this->language = $lang;
    $this->doctype = $doctype;
    $this->document = json_decode($docdata);
  }

  public function getText() {
    return $this->document->{'text'};
  }
}

?>