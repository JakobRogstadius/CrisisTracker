CREATE DATABASE `AidrPredict` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE AidrPredict;

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Temporary table structure for view `active_attributes`
--

DROP TABLE IF EXISTS `active_attributes`;
/*!50001 DROP VIEW IF EXISTS `active_attributes`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `active_attributes` (
  `crisisID` tinyint NOT NULL,
  `attributeInfo` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Temporary table structure for view `active_attributes_inner`
--

DROP TABLE IF EXISTS `active_attributes_inner`;
/*!50001 DROP VIEW IF EXISTS `active_attributes_inner`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `active_attributes_inner` (
  `crisisID` tinyint NOT NULL,
  `nominalAttributeID` tinyint NOT NULL,
  `name` tinyint NOT NULL,
  `description` tinyint NOT NULL,
  `labels` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `users` (
  `userID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `role` varchar(45) NOT NULL DEFAULT 'normal',
  PRIMARY KEY (`userID`),
  UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO users (`userID`, `name`, `role`) VALUES (0, 'SYSTEM', 'admin');

--
-- Table structure for table `crisis`
--

DROP TABLE IF EXISTS `crisis`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `crisis` (
  `crisisID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(140) NOT NULL DEFAULT 'unnamed crisis',
  `crisisTypeID` int(10) unsigned NOT NULL,
  `code` varchar(45) NOT NULL,
  `userID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`crisisID`),
  UNIQUE KEY `code_UNIQUE` (`code`),
  KEY `crisisTypeID_idx` (`crisisTypeID`),
  KEY `fk_crisis_users_userID_idx` (`userID`),
  CONSTRAINT `fk_Crisis_CrisisType_crisisTypeID` FOREIGN KEY (`crisisTypeID`) REFERENCES `crisis_type` (`crisisTypeID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_crisis_users_userID` FOREIGN KEY (`userID`) REFERENCES `users` (`userID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `crisis_nominal_attribute`
--

DROP TABLE IF EXISTS `crisis_nominal_attribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `crisis_nominal_attribute` (
  `crisisID` int(11) unsigned NOT NULL,
  `nominalAttributeID` int(11) unsigned NOT NULL,
  PRIMARY KEY (`crisisID`,`nominalAttributeID`),
  KEY `crisisID_idx` (`crisisID`),
  KEY `nominalAttributeID_idx` (`nominalAttributeID`),
  CONSTRAINT `fk_Crisis_NominalAttribute_crisisID` FOREIGN KEY (`crisisID`) REFERENCES `crisis` (`crisisID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_Crisis_NominalAttribute_nominalAttributeID` FOREIGN KEY (`nominalAttributeID`) REFERENCES `nominal_attribute` (`nominalAttributeID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `crisis_type`
--

DROP TABLE IF EXISTS `crisis_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `crisis_type` (
  `crisisTypeID` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(140) NOT NULL DEFAULT 'default type',
  PRIMARY KEY (`crisisTypeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `document`
--

DROP TABLE IF EXISTS `document`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `document` (
  `documentID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `crisisID` int(10) unsigned NOT NULL,
  `isEvaluationSet` bit(1) NOT NULL DEFAULT b'0',
  `hasHumanLabels` bit(1) NOT NULL DEFAULT b'0',
  `sourceIP` int(10) unsigned NOT NULL,
  `valueAsTrainingSample` double unsigned NOT NULL DEFAULT '0',
  `receivedAt` datetime NOT NULL,
  `language` varchar(5) NOT NULL DEFAULT 'en',
  `doctype` varchar(20) NOT NULL,
  `data` text NOT NULL,
  `wordFeatures` text,
  `geoFeatures` text,
  PRIMARY KEY (`documentID`),
  KEY `fk_Document_Crisis_crisisID_idx` (`crisisID`),
  KEY `isEvaluationSet` (`isEvaluationSet`),
  CONSTRAINT `fk_Document_Crisis_crisisID` FOREIGN KEY (`crisisID`) REFERENCES `crisis` (`crisisID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `document_BINS`
BEFORE INSERT ON `document`
FOR EACH ROW
set new.isEvaluationSet = mod((SELECT AUTO_INCREMENT FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='document'), 5)=0 */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `document_nominal_label`
--

DROP TABLE IF EXISTS `document_nominal_label`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `document_nominal_label` (
  `documentID` bigint(20) unsigned NOT NULL,
  `nominalLabelID` int(10) unsigned NOT NULL,
  `timestamp` datetime NOT NULL,
  PRIMARY KEY (`documentID`,`nominalLabelID`),
  KEY `fk_Document_NominalLabel_documentID_idx` (`documentID`),
  KEY `fk_document_nominal_label_nominal_label_idx` (`nominalLabelID`),
  CONSTRAINT `fk_Document_NominalLabel_documentID` FOREIGN KEY (`documentID`) REFERENCES `document` (`documentID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_document_nominal_label_nominal_label` FOREIGN KEY (`nominalLabelID`) REFERENCES `nominal_label` (`nominalLabelID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `model`
--

DROP TABLE IF EXISTS `model`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `model` (
  `modelID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `modelFamilyID` int(10) unsigned NOT NULL,
  `avgPrecision` double unsigned NOT NULL,
  `avgRecall` double unsigned NOT NULL,
  `avgAuc` double unsigned NOT NULL,
  `trainingCount` int(10) unsigned NOT NULL,
  `trainingTime` datetime NOT NULL,
  `isCurrentModel` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`modelID`),
  KEY `fk_Model_ModelFamily_modelFamilyID_idx` (`modelFamilyID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `model_family`
--

DROP TABLE IF EXISTS `model_family`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `model_family` (
  `modelFamilyID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `crisisID` int(10) unsigned NOT NULL,
  `nominalAttributeID` int(10) unsigned NOT NULL,
  `isActive` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`modelFamilyID`),
  UNIQUE KEY `unique` (`crisisID`,`nominalAttributeID`),
  KEY `fk_ModelFamily_Crisis_idx` (`crisisID`),
  KEY `fk_ModelFamily_NominalAttribute_idx` (`nominalAttributeID`),
  CONSTRAINT `fk_ModelFamily_Crisis` FOREIGN KEY (`crisisID`) REFERENCES `crisis` (`crisisID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_ModelFamily_NominalAttribute` FOREIGN KEY (`nominalAttributeID`) REFERENCES `nominal_attribute` (`nominalAttributeID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `model_nominal_label`
--

DROP TABLE IF EXISTS `model_nominal_label`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `model_nominal_label` (
  `modelID` int(10) unsigned NOT NULL,
  `nominalLabelID` int(10) unsigned NOT NULL,
  `labelPrecision` double DEFAULT NULL,
  `labelRecall` double DEFAULT NULL,
  `labelAuc` double DEFAULT NULL,
  `classifiedDocumentCount` int(11) DEFAULT NULL,
  PRIMARY KEY (`modelID`,`nominalLabelID`),
  KEY `fk_model_label_modelID_idx` (`modelID`),
  KEY `fk_model_nominal_label_nominal_label_idx` (`nominalLabelID`),
  CONSTRAINT `fk_model_modellabel_modelID` FOREIGN KEY (`modelID`) REFERENCES `model` (`modelID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_model_nominal_label_nominal_label` FOREIGN KEY (`nominalLabelID`) REFERENCES `nominal_label` (`nominalLabelID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `nominal_attribute`
--

DROP TABLE IF EXISTS `nominal_attribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `nominal_attribute` (
  `nominalAttributeID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `userID` int(10) unsigned NOT NULL DEFAULT '0',
  `name` varchar(140) NOT NULL,
  `description` varchar(600) NOT NULL DEFAULT '',
  `code` varchar(15) NOT NULL,
  PRIMARY KEY (`nominalAttributeID`),
  UNIQUE KEY `code_UNIQUE` (`code`),
  KEY `fk_nominalAttribute_users_userID_idx` (`userID`),
  CONSTRAINT `fk_nominalAttribute_users_userID` FOREIGN KEY (`userID`) REFERENCES `users` (`userID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `nominal_label`
--

DROP TABLE IF EXISTS `nominal_label`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `nominal_label` (
  `nominalLabelID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `nominalLabelCode` varchar(15) NOT NULL,
  `nominalAttributeID` int(10) unsigned NOT NULL,
  `name` varchar(45) NOT NULL,
  `description` varchar(600) NOT NULL DEFAULT '',
  PRIMARY KEY (`nominalLabelID`),
  UNIQUE KEY `attribute_labelcode_unique` (`nominalLabelCode`,`nominalAttributeID`),
  KEY `asd_idx` (`nominalAttributeID`),
  CONSTRAINT `fk_NominalLabel_NominalAttribute_nominalAttributeID` FOREIGN KEY (`nominalAttributeID`) REFERENCES `nominal_attribute` (`nominalAttributeID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `nominal_label_evaluation_data`
--

DROP TABLE IF EXISTS `nominal_label_evaluation_data`;
/*!50001 DROP VIEW IF EXISTS `nominal_label_evaluation_data`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `nominal_label_evaluation_data` (
  `documentID` tinyint NOT NULL,
  `crisisID` tinyint NOT NULL,
  `nominalLabelID` tinyint NOT NULL,
  `nominalAttributeID` tinyint NOT NULL,
  `wordFeatures` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Temporary table structure for view `nominal_label_training_data`
--

DROP TABLE IF EXISTS `nominal_label_training_data`;
/*!50001 DROP VIEW IF EXISTS `nominal_label_training_data`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `nominal_label_training_data` (
  `documentID` tinyint NOT NULL,
  `crisisID` tinyint NOT NULL,
  `nominalLabelID` tinyint NOT NULL,
  `nominalAttributeID` tinyint NOT NULL,
  `wordFeatures` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `task_answer`
--

DROP TABLE IF EXISTS `task_answer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `task_answer` (
  `documentID` bigint(20) unsigned NOT NULL,
  `userID` int(10) unsigned NOT NULL,
  `answer` text NOT NULL,
  `timestamp` datetime NOT NULL,
  `fromTrustedUser` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`documentID`,`userID`),
  KEY `fk_TaskAnswer_Document_idx` (`documentID`),
  CONSTRAINT `fk_TaskAnswer_Document` FOREIGN KEY (`documentID`) REFERENCES `document` (`documentID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `task_assignment`
--

DROP TABLE IF EXISTS `task_assignment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `task_assignment` (
  `documentID` bigint(20) unsigned NOT NULL,
  `userID` int(10) unsigned NOT NULL,
  `assignedAt` datetime NOT NULL,
  PRIMARY KEY (`documentID`,`userID`),
  KEY `fk_TaskAssignment_Document_idx` (`documentID`),
  CONSTRAINT `fk_TaskAssignment_Document` FOREIGN KEY (`documentID`) REFERENCES `document` (`documentID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `task_buffer`
--

DROP TABLE IF EXISTS `task_buffer`;
/*!50001 DROP VIEW IF EXISTS `task_buffer`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `task_buffer` (
  `documentID` tinyint NOT NULL,
  `crisisID` tinyint NOT NULL,
  `attributeInfo` tinyint NOT NULL,
  `language` tinyint NOT NULL,
  `doctype` tinyint NOT NULL,
  `data` tinyint NOT NULL,
  `valueAsTrainingSample` tinyint NOT NULL,
  `assignedCount` tinyint NOT NULL
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;


--
-- Final view structure for view `active_attributes`
--

/*!50001 DROP TABLE IF EXISTS `active_attributes`*/;
/*!50001 DROP VIEW IF EXISTS `active_attributes`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50001 VIEW `active_attributes` AS select `active_attributes_inner`.`crisisID` AS `crisisID`,concat('{"attributes":[',group_concat(concat('{"id":',`active_attributes_inner`.`nominalAttributeID`,',"name":"',`active_attributes_inner`.`name`,'","description":"',`active_attributes_inner`.`description`,'",',`active_attributes_inner`.`labels`,'}') separator ','),']}') AS `attributeInfo` from `active_attributes_inner` group by `active_attributes_inner`.`crisisID` */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `active_attributes_inner`
--

/*!50001 DROP TABLE IF EXISTS `active_attributes_inner`*/;
/*!50001 DROP VIEW IF EXISTS `active_attributes_inner`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50001 VIEW `active_attributes_inner` AS select `m`.`crisisID` AS `crisisID`,`m`.`nominalAttributeID` AS `nominalAttributeID`,`a`.`name` AS `name`,`a`.`description` AS `description`,concat('"labels":[',group_concat(concat('{"name":"',`l`.`name`,'","description":"',`l`.`description`,'","code":"',`l`.`nominalLabelCode`,'","id":',`l`.`nominalLabelID`,'}') order by (`l`.`nominalLabelCode` = 'null') DESC,`l`.`name` ASC separator ','),']') AS `labels` from ((`model_family` `m` join `nominal_attribute` `a` on((`a`.`nominalAttributeID` = `m`.`nominalAttributeID`))) join `nominal_label` `l` on((`l`.`nominalAttributeID` = `m`.`nominalAttributeID`))) where `m`.`isActive` group by `m`.`crisisID`,`m`.`nominalAttributeID` */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `nominal_label_evaluation_data`
--

/*!50001 DROP TABLE IF EXISTS `nominal_label_evaluation_data`*/;
/*!50001 DROP VIEW IF EXISTS `nominal_label_evaluation_data`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50001 VIEW `nominal_label_evaluation_data` AS select `d`.`documentID` AS `documentID`,`d`.`crisisID` AS `crisisID`,`dnl`.`nominalLabelID` AS `nominalLabelID`,`nl`.`nominalAttributeID` AS `nominalAttributeID`,`d`.`wordFeatures` AS `wordFeatures` from ((`document` `d` join `document_nominal_label` `dnl` on((`d`.`documentID` = `dnl`.`documentID`))) join `nominal_label` `nl` on((`nl`.`nominalLabelID` = `dnl`.`nominalLabelID`))) where (`d`.`isEvaluationSet` and (`d`.`wordFeatures` is not null)) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `nominal_label_training_data`
--

/*!50001 DROP TABLE IF EXISTS `nominal_label_training_data`*/;
/*!50001 DROP VIEW IF EXISTS `nominal_label_training_data`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50001 VIEW `nominal_label_training_data` AS select `d`.`documentID` AS `documentID`,`d`.`crisisID` AS `crisisID`,`dnl`.`nominalLabelID` AS `nominalLabelID`,`nl`.`nominalAttributeID` AS `nominalAttributeID`,`d`.`wordFeatures` AS `wordFeatures` from ((`document` `d` join `document_nominal_label` `dnl` on((`d`.`documentID` = `dnl`.`documentID`))) join `nominal_label` `nl` on((`nl`.`nominalLabelID` = `dnl`.`nominalLabelID`))) where ((not(`d`.`isEvaluationSet`)) and (`d`.`wordFeatures` is not null)) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `task_buffer`
--

/*!50001 DROP TABLE IF EXISTS `task_buffer`*/;
/*!50001 DROP VIEW IF EXISTS `task_buffer`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_general_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50001 VIEW `task_buffer` AS select `d`.`documentID` AS `documentID`,`d`.`crisisID` AS `crisisID`,`active_attributes`.`attributeInfo` AS `attributeInfo`,`d`.`language` AS `language`,`d`.`doctype` AS `doctype`,`d`.`data` AS `data`,`d`.`valueAsTrainingSample` AS `valueAsTrainingSample`,sum((`asg`.`documentID` is not null)) AS `assignedCount` from ((`document` `d` join `active_attributes` on((`d`.`crisisID` = `active_attributes`.`crisisID`))) left join `task_assignment` `asg` on((`asg`.`documentID` = `d`.`documentID`))) where (not(`d`.`hasHumanLabels`)) group by `d`.`documentID` order by `d`.`crisisID`,`d`.`valueAsTrainingSample` desc,`d`.`documentID` desc */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2013-07-29 16:17:56
