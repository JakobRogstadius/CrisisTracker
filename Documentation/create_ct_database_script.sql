CREATE DATABASE `CrisisTracker` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE CrisisTracker;

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
-- Table structure for table `AidrAttribute`
--

DROP TABLE IF EXISTS `AidrAttribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AidrAttribute` (
  `AttributeID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `AttributeCode` varchar(45) NOT NULL,
  `AttributeName` varchar(200) NOT NULL,
  `AttributeDescription` text,
  PRIMARY KEY (`AttributeID`),
  UNIQUE KEY `AttributeCode_UNIQUE` (`AttributeCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AidrLabel`
--

DROP TABLE IF EXISTS `AidrLabel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AidrLabel` (
  `LabelID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `AttributeID` int(10) unsigned NOT NULL,
  `LabelCode` varchar(45) NOT NULL,
  `LabelName` varchar(200) NOT NULL,
  `LabelDescription` text,
  PRIMARY KEY (`LabelID`),
  UNIQUE KEY `ix_AidrLabel_unique` (`AttributeID`,`LabelCode`),
  KEY `fk_AidrLabel_AidrAttribute_idx` (`AttributeID`),
  CONSTRAINT `fk_AidrLabel_AidrAttribute` FOREIGN KEY (`AttributeID`) REFERENCES `AidrAttribute` (`AttributeID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='	';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Constants`
--

DROP TABLE IF EXISTS `Constants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Constants` (
  `Name` varchar(30) NOT NULL,
  `Value` double NOT NULL,
  PRIMARY KEY (`Name`),
  KEY `Full` (`Name`,`Value`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO Constants (Name, Value) VALUES ('WordScore4dHigh', 100);

--
-- Table structure for table `HourStatistics`
--

DROP TABLE IF EXISTS `HourStatistics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `HourStatistics` (
  `DateHour` datetime NOT NULL,
  `TweetsProcessed` int(10) unsigned NOT NULL DEFAULT '0',
  `TweetsDiscarded` int(10) unsigned NOT NULL DEFAULT '0',
  `TweetClustersCreated` int(10) unsigned NOT NULL DEFAULT '0',
  `StoriesCreated` int(10) unsigned NOT NULL DEFAULT '0',
  `StoriesAutoMerged` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`DateHour`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `InfoCategory`
--

DROP TABLE IF EXISTS `InfoCategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `InfoCategory` (
  `InfoCategoryID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Category` varchar(30) NOT NULL,
  PRIMARY KEY (`InfoCategoryID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `InfoEntity`
--

DROP TABLE IF EXISTS `InfoEntity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `InfoEntity` (
  `InfoEntityID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Entity` varchar(45) NOT NULL,
  PRIMARY KEY (`InfoEntityID`),
  UNIQUE KEY `Entity_UNIQUE` (`Entity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `InfoKeyword`
--

DROP TABLE IF EXISTS `InfoKeyword`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `InfoKeyword` (
  `InfoKeywordID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Keyword` varchar(30) NOT NULL,
  PRIMARY KEY (`InfoKeywordID`),
  UNIQUE KEY `Keyword_UNIQUE` (`Keyword`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Story`
--

DROP TABLE IF EXISTS `Story`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Story` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `TweetCount` int(10) unsigned NOT NULL DEFAULT '0',
  `RetweetCount` int(10) unsigned NOT NULL DEFAULT '0',
  `UserCount` int(10) unsigned NOT NULL DEFAULT '0',
  `TopUserCount` int(10) unsigned NOT NULL DEFAULT '0',
  `TopUserCountRecent` int(10) unsigned NOT NULL DEFAULT '0',
  `WeightedSize` double unsigned NOT NULL DEFAULT '0',
  `WeightedSizeRecent` double unsigned NOT NULL DEFAULT '0',
  `StartTime` datetime DEFAULT NULL,
  `EndTime` datetime DEFAULT NULL,
  `IsArchived` BIT(1) NOT NULL DEFAULT 0,
  `TagScore` double unsigned NOT NULL DEFAULT '0',
  `PendingUpdate` bit(1) NOT NULL DEFAULT b'0',
  `IsHidden` bit(1) NOT NULL DEFAULT b'0',
  `Latitude` double DEFAULT NULL,
  `Longitude` double DEFAULT NULL,
  `Title` varchar(140) DEFAULT NULL,
  PRIMARY KEY (`StoryID`),
  KEY `Archived` (`IsArchived`),
  KEY `Time` (`StartTime`),
  KEY `IsHidden` (`IsHidden`),
  KEY `Importance` (`WeightedSize`),
  KEY `ImportanceRecent` (`WeightedSizeRecent`),
  KEY `PendingUpdate` (`PendingUpdate`),
  KEY `TopUserCount` (`TopUserCount`)
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
/*!50003 CREATE*/ /*!50003 TRIGGER `Story_Insert`
AFTER INSERT ON `Story`
FOR EACH ROW
BEGIN
    INSERT INTO HourStatistics (DateHour, StoriesCreated)
    VALUES (DATE_FORMAT(utc_timestamp(), "%Y-%m-%d %H:00:00"), 1)
    ON DUPLICATE KEY UPDATE StoriesCreated = StoriesCreated + 1;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `StoryAidrAttributeTag`
--

DROP TABLE IF EXISTS `StoryAidrAttributeTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryAidrAttributeTag` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `LabelID` int(10) unsigned NOT NULL,
  `TagCount` int(10) unsigned NOT NULL,
  `IsMajorityTag` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`StoryID`,`LabelID`),
  KEY `fk_StoryAidrTopicTag_StoryID_idx` (`StoryID`),
  KEY `IsMajorityTag` (`IsMajorityTag`),
  KEY `TweetCount` (`TagCount`),
  KEY `fk_StoryAidrAttributeTag_AidrLabel_idx` (`LabelID`),
  CONSTRAINT `fk_StoryAidrAttributeTag_AidrLabel` FOREIGN KEY (`LabelID`) REFERENCES `AidrLabel` (`LabelID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_StoryAidrAttributeTag_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoryCustomTitle`
--

DROP TABLE IF EXISTS `StoryCustomTitle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryCustomTitle` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `CustomTitle` text NOT NULL,
  PRIMARY KEY (`StoryID`),
  KEY `StoryID` (`StoryID`),
  CONSTRAINT `StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoryInfoCategoryTag`
--

DROP TABLE IF EXISTS `StoryInfoCategoryTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryInfoCategoryTag` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `InfoCategoryID` int(10) unsigned NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `UserID` bigint(20) unsigned NOT NULL,
  `IP` int(11) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`StoryID`,`InfoCategoryID`),
  KEY `Reverse` (`InfoCategoryID`,`StoryID`),
  KEY `User` (`UserID`),
  KEY `StoryID` (`StoryID`),
  KEY `InfoCategoryTag` (`InfoCategoryID`),
  KEY `StoryInfoCategoryTag_TagID` (`InfoCategoryID`),
  CONSTRAINT `StoryInfoCategoryTag_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `StoryInfoCategoryTag_TagID` FOREIGN KEY (`InfoCategoryID`) REFERENCES `InfoCategory` (`InfoCategoryID`) ON DELETE CASCADE ON UPDATE NO ACTION
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
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoCategoryTag_Insert`
AFTER INSERT ON `StoryInfoCategoryTag`
FOR EACH ROW
update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoCategoryTag_UPDATE`
AFTER UPDATE ON `StoryInfoCategoryTag`
FOR EACH ROW
BEGIN
    update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID;
    update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoCategoryTag_Delete`
AFTER DELETE ON `StoryInfoCategoryTag`
FOR EACH ROW
update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `StoryInfoEntityTag`
--

DROP TABLE IF EXISTS `StoryInfoEntityTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryInfoEntityTag` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `InfoEntityID` int(10) unsigned NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `UserID` bigint(20) unsigned NOT NULL,
  `IP` int(11) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`StoryID`,`InfoEntityID`),
  KEY `UserID` (`UserID`),
  KEY `Reverse` (`InfoEntityID`,`StoryID`),
  KEY `StoryInfoEntityTag_StoryID` (`StoryID`),
  KEY `StoryInfoEntityTag_TagID` (`InfoEntityID`),
  CONSTRAINT `StoryInfoEntityTag_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `StoryInfoEntityTag_TagID` FOREIGN KEY (`InfoEntityID`) REFERENCES `InfoEntity` (`InfoEntityID`) ON DELETE CASCADE ON UPDATE NO ACTION
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
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoEntityTag_Insert`
AFTER INSERT ON `StoryInfoEntityTag`
FOR EACH ROW
update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoEntityTag_Update`
AFTER UPDATE ON `StoryInfoEntityTag`
FOR EACH ROW
BEGIN
    update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID;
    update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryInfoEntityTag_Delete`
AFTER DELETE ON `StoryInfoEntityTag`
FOR EACH ROW
update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `StoryInfoKeywordTag`
--

DROP TABLE IF EXISTS `StoryInfoKeywordTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryInfoKeywordTag` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `InfoKeywordID` int(10) unsigned NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `UserID` bigint(20) unsigned NOT NULL,
  `IP` int(10) unsigned NOT NULL DEFAULT '0',
  `Weight` double unsigned NOT NULL,
  PRIMARY KEY (`StoryID`,`InfoKeywordID`),
  KEY `Reverse` (`InfoKeywordID`,`StoryID`),
  KEY `User` (`UserID`),
  KEY `StoryInfoKeywordTag_StoryID` (`StoryID`),
  KEY `StoryInfoKeywordTag_TagID` (`InfoKeywordID`),
  KEY `Weight` (`Weight`),
  CONSTRAINT `StoryInfoKeywordTag_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `StoryInfoKeywordTag_TagID` FOREIGN KEY (`InfoKeywordID`) REFERENCES `InfoKeyword` (`InfoKeywordID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoryLocationTag`
--

DROP TABLE IF EXISTS `StoryLocationTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryLocationTag` (
  `TagID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `StoryID` bigint(20) unsigned NOT NULL,
  `Longitude` decimal(9,6) NOT NULL DEFAULT '0.000000',
  `Latitude` decimal(9,6) NOT NULL DEFAULT '0.000000',
  `CreatedAt` datetime NOT NULL,
  `UserID` bigint(20) unsigned NOT NULL,
  `IP` int(11) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`TagID`),
  UNIQUE KEY `Geo` (`StoryID`,`Longitude`,`Latitude`),
  KEY `User` (`UserID`),
  KEY `StoryLocationTag_StoryID` (`StoryID`),
  CONSTRAINT `StoryLocationTag_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION
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
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryLocationTag_Insert`
AFTER INSERT ON `StoryLocationTag`
FOR EACH ROW
update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryLocationTag_Update`
AFTER UPDATE ON `StoryLocationTag`
FOR EACH ROW
BEGIN
update Story set TagScore := TagScore+1 where Story.StoryID = new.StoryID;
update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50003 TRIGGER `StoryLocationTag_Delete`
AFTER DELETE ON `StoryLocationTag`
FOR EACH ROW
update Story set TagScore := TagScore-1 where Story.StoryID = old.StoryID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `StoryLog`
--

DROP TABLE IF EXISTS `StoryLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryLog` (
  `LogID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `IP` int(10) unsigned NOT NULL DEFAULT '0',
  `UserID` bigint(20) unsigned NOT NULL DEFAULT '0',
  `Timestamp` datetime NOT NULL,
  `EventType` tinyint(3) unsigned NOT NULL,
  `StoryID` bigint(20) unsigned NOT NULL,
  `StoryAgeInSeconds` int(10) unsigned DEFAULT NULL,
  `MergedWithStoryID` bigint(20) unsigned DEFAULT NULL,
  `TweetCount` int(10) unsigned DEFAULT NULL,
  `RetweetCount` int(10) unsigned DEFAULT NULL,
  `UserCount` int(10) unsigned DEFAULT NULL,
  `TopUserCount` int(10) unsigned DEFAULT NULL,
  `Trend` int(10) unsigned DEFAULT NULL,
  `TagID` bigint(20) unsigned DEFAULT NULL,
  PRIMARY KEY (`LogID`),
  KEY `UserID` (`UserID`,`EventType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoryLogEventType`
--

DROP TABLE IF EXISTS `StoryLogEventType`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryLogEventType` (
  `EventType` int(11) NOT NULL,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`EventType`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoryMerges`
--

DROP TABLE IF EXISTS `StoryMerges`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoryMerges` (
  `StoryID1` bigint(20) unsigned NOT NULL,
  `StoryID2` bigint(20) unsigned NOT NULL,
  `MergedAt` datetime DEFAULT NULL,
  `IP` int(10) unsigned NOT NULL DEFAULT '0',
  `UserID` bigint(20) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`StoryID1`,`StoryID2`),
  KEY `PendingStoryMerges_StoryID1` (`StoryID1`),
  KEY `PendingStoryMerges_StoryID2` (`StoryID2`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StorySplits`
--

DROP TABLE IF EXISTS `StorySplits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StorySplits` (
  `StoryID` bigint(20) unsigned NOT NULL,
  `TweetClusterID` bigint(20) unsigned NOT NULL,
  `SplitAt` datetime DEFAULT NULL,
  `IP` int(10) unsigned NOT NULL DEFAULT '0',
  `UserID` bigint(20) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`StoryID`,`TweetClusterID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Tweet`
--

DROP TABLE IF EXISTS `Tweet`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Tweet` (
  `TweetID` bigint(20) unsigned NOT NULL,
  `UserID` bigint(20) unsigned NOT NULL,
  `CreatedAt` datetime NOT NULL,
  `CalculatedRelations` bit(1) NOT NULL DEFAULT b'0',
  `ProcessedMetatags` bit(1) NOT NULL DEFAULT b'0',
  `Novelty` double NOT NULL DEFAULT '1',
  `TweetClusterID` bigint(20) unsigned DEFAULT NULL,
  `RetweetOf` bigint(20) unsigned DEFAULT NULL,
  `RetweetOfUserID` bigint(20) unsigned DEFAULT NULL,
  `Longitude` float DEFAULT NULL,
  `Latitude` float DEFAULT NULL,
  `TextHash` int(11) NOT NULL,
  `Language` varchar(7) NOT NULL DEFAULT 'und',
  `Text` varchar(140) DEFAULT NULL,
  PRIMARY KEY (`TweetID`),
  KEY `CreatedAt` (`CreatedAt`),
  KEY `Relations` (`CalculatedRelations`),
  KEY `Retweet` (`RetweetOf`),
  KEY `Location` (`Longitude`,`Latitude`),
  KEY `User` (`UserID`),
  KEY `Tweet_TweetClusterID` (`TweetClusterID`),
  KEY `TextHash` (`TextHash`),
  CONSTRAINT `Tweet_TweetClusterID` FOREIGN KEY (`TweetClusterID`) REFERENCES `TweetCluster` (`TweetClusterID`) ON DELETE CASCADE ON UPDATE NO ACTION
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
/*!50003 CREATE*/ /*!50003 TRIGGER `Tweet_Delete`
AFTER DELETE ON `Tweet`
FOR EACH ROW
delete from TweetUrl where TweetID = old.TweetID */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `TweetAidrAttributeTag`
--

DROP TABLE IF EXISTS `TweetAidrAttributeTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetAidrAttributeTag` (
  `TweetID` bigint(20) unsigned NOT NULL,
  `LabelID` int(10) unsigned NOT NULL,
  `Confidence` double unsigned NOT NULL,
  PRIMARY KEY (`TweetID`,`LabelID`),
  KEY `fk_TweetAidrAttributeTag_Tweet_idx` (`TweetID`),
  KEY `fk_TweetAidrAttributeTag_AidrLabel_idx` (`LabelID`),
  CONSTRAINT `fk_TweetAidrAttributeTag_AidrLabel` FOREIGN KEY (`LabelID`) REFERENCES `AidrLabel` (`LabelID`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_TweetAidrAttributeTag_Tweet` FOREIGN KEY (`TweetID`) REFERENCES `Tweet` (`TweetID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TweetCluster`
--

DROP TABLE IF EXISTS `TweetCluster`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetCluster` (
  `TweetClusterID` bigint(20) unsigned NOT NULL,
  `StoryID` bigint(20) unsigned DEFAULT NULL,
  `StartTime` datetime NOT NULL,
  `EndTime` datetime NOT NULL,
  `PendingClusterUpdate` bit(1) NOT NULL DEFAULT b'0',
  `PendingStoryUpdate` bit(1) NOT NULL DEFAULT b'0',
  `IsArchived` bit(1) NOT NULL DEFAULT b'0',
  `TweetCount` int(10) unsigned NOT NULL DEFAULT '0',
  `Title` varchar(140) DEFAULT NULL,
  PRIMARY KEY (`TweetClusterID`),
  KEY `Archived` (`IsArchived`),
  KEY `StartTime` (`StartTime`),
  KEY `EndTime` (`EndTime`),
  KEY `TweetCluster_StoryID` (`StoryID`),
  KEY `PendingClusterUpdate` (`PendingClusterUpdate`),
  KEY `PendingStoryUpdate` (`PendingStoryUpdate`),
  KEY `TweetCount` (`TweetCount`),
  CONSTRAINT `TweetCluster_StoryID` FOREIGN KEY (`StoryID`) REFERENCES `Story` (`StoryID`) ON DELETE CASCADE ON UPDATE NO ACTION
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
/*!50003 CREATE*/ /*!50003 TRIGGER `TweetCluster_Insert`
AFTER INSERT ON `TweetCluster`
FOR EACH ROW
BEGIN
    INSERT INTO HourStatistics (DateHour, TweetClustersCreated)
    VALUES (DATE_FORMAT(utc_timestamp(), "%Y-%m-%d %H:00:00"), 1)
    ON DUPLICATE KEY UPDATE TweetClustersCreated = TweetClustersCreated + 1;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `TweetClusterCollision`
--

DROP TABLE IF EXISTS `TweetClusterCollision`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetClusterCollision` (
  `CollisionID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `TweetClusterID1` bigint(20) unsigned NOT NULL,
  `TweetClusterID2` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`CollisionID`),
  UNIQUE KEY `TweetClusterCollision_Unique` (`TweetClusterID1`,`TweetClusterID2`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TweetJson`
--

DROP TABLE IF EXISTS `TweetJson`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetJson` (
  `ID` bigint(20) NOT NULL AUTO_INCREMENT,
  `WordStatsOnly` bit(1) NOT NULL,
  `Json` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TweetRelation`
--

DROP TABLE IF EXISTS `TweetRelation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetRelation` (
  `TweetID1` bigint(20) unsigned NOT NULL,
  `TweetID2` bigint(20) unsigned NOT NULL,
  `Similarity` float unsigned NOT NULL DEFAULT '0.1',
  PRIMARY KEY (`TweetID1`,`TweetID2`),
  KEY `TweetRelation_TweetID1` (`TweetID1`),
  KEY `TweetRelation_TweetID2` (`TweetID2`),
  CONSTRAINT `TweetRelation_TweetID1` FOREIGN KEY (`TweetID1`) REFERENCES `Tweet` (`TweetID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `TweetRelation_TweetID2` FOREIGN KEY (`TweetID2`) REFERENCES `Tweet` (`TweetID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TweetRelationInsertBuffer`
--

DROP TABLE IF EXISTS `TweetRelationInsertBuffer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetRelationInsertBuffer` (
  `TweetID1` bigint(20) unsigned NOT NULL,
  `TweetID2` bigint(20) unsigned NOT NULL,
  `Similarity` float unsigned NOT NULL DEFAULT '0.1',
  PRIMARY KEY (`TweetID1`,`TweetID2`),
  KEY `TweetRelationInsertBuffer_TweetID1` (`TweetID1`),
  KEY `TweetRelationInsertBuffer_TweetID2` (`TweetID2`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TweetUrl`
--

DROP TABLE IF EXISTS `TweetUrl`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TweetUrl` (
  `TweetID` bigint(20) unsigned NOT NULL,
  `UrlHash` int(11) NOT NULL,
  `Url` varchar(255) CHARACTER SET latin1 NOT NULL,
  PRIMARY KEY (`TweetID`,`UrlHash`),
  KEY `UrlHash` (`UrlHash`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TwitterTrackFilter`
--

DROP TABLE IF EXISTS `TwitterTrackFilter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TwitterTrackFilter` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `IsActive` bit(1) NOT NULL DEFAULT b'1',
  `IsStrong` bit(1) NOT NULL DEFAULT 1,
  `Hits1d` double unsigned NOT NULL DEFAULT '0',
  `Discards1d` double unsigned NOT NULL DEFAULT '0',
  `FilterType` TINYINT(3) UNSIGNED NOT NULL DEFAULT 0,
  `Word` varchar(45) DEFAULT NULL,
  `UserID` bigint(20) unsigned DEFAULT NULL,
  `UserName` varchar(45) DEFAULT NULL,
  `Region` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `Keyword` (`Word`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TwitterUser`
--

DROP TABLE IF EXISTS `TwitterUser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `TwitterUser` (
  `UserID` bigint(20) unsigned NOT NULL,
  `IsTopUser` bit(1) NOT NULL DEFAULT b'0',
  `IsBlacklisted` bit(1) NOT NULL DEFAULT b'0',
  `ScreenName` varchar(45) NOT NULL,
  `RealName` varchar(45) NOT NULL,
  `ProfileImageUrl` varchar(200) DEFAULT NULL,
  `Score12h` double unsigned NOT NULL DEFAULT '1',
  PRIMARY KEY (`UserID`),
  KEY `Score` (`Score12h`),
  KEY `IsTopUser` (`IsTopUser`),
  KEY `IsBlacklisted` (`IsBlacklisted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `User` (
  `UserID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `TwitterUserID` bigint(20) unsigned DEFAULT NULL,
  `Name` varchar(45) DEFAULT NULL,
  `IsAnonymous` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `TwitterUserID_UNIQUE` (`TwitterUserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Word`
--

DROP TABLE IF EXISTS `Word`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Word` (
  `Word` varchar(30) NOT NULL,
  `WordID` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`Word`),
  KEY `WordID` (`WordID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `WordScore`
--

DROP TABLE IF EXISTS `WordScore`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `WordScore` (
  `WordID` bigint(20) unsigned NOT NULL,
  `Score4d` double unsigned NOT NULL DEFAULT '1',
  `Score1h` double unsigned NOT NULL DEFAULT '1',
  PRIMARY KEY (`WordID`),
  KEY `WordScore_WordID` (`WordID`),
  CONSTRAINT `WordScore_WordID` FOREIGN KEY (`WordID`) REFERENCES `Word` (`WordID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `WordTweet`
--

DROP TABLE IF EXISTS `WordTweet`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `WordTweet` (
  `WordID` bigint(20) unsigned NOT NULL,
  `TweetID` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`WordID`,`TweetID`),
  KEY `Reverse` (`TweetID`,`WordID`),
  KEY `WordTweet_TweetID` (`TweetID`),
  KEY `WordTweet_WordID` (`WordID`),
  CONSTRAINT `WordTweet_TweetID` FOREIGN KEY (`TweetID`) REFERENCES `Tweet` (`TweetID`) ON DELETE CASCADE ON UPDATE NO ACTION,
  CONSTRAINT `WordTweet_WordID` FOREIGN KEY (`WordID`) REFERENCES `Word` (`WordID`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlaceName`
--

DROP TABLE IF EXISTS `PlaceName`;
CREATE TABLE `PlaceName` (
  `PlaceNameID` int(11) NOT NULL AUTO_INCREMENT,
  `PlaceName` varchar(100) NOT NULL,
  `Latitude` double NOT NULL,
  `Longitude` double NOT NULL,
  PRIMARY KEY (`PlaceNameID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

delimiter $$

CREATE PROCEDURE `AddRemoveStoryCustomTitle`(usrID bigint, usrIP int, sID bigint, customTitle text)
BEGIN
IF !isnull(customTitle) THEN
    insert into StoryCustomTitle (StoryID, CustomTitle) values (sID, customTitle)
    on duplicate key update CustomTitle=VALUES(CustomTitle);
ELSE
    delete from StoryCustomTitle where StoryID=sID limit 1;
END IF;

IF row_count()=1 THEN
    insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend)
    select usrIP, usrID, utc_timestamp(),
        case when isnull(customTitle) then 44 else 34 end,
        unix_timestamp(utc_timestamp())-unix_timestamp(StartTime),
        StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend
    from Story where StoryID=sID;
END IF;
END$$

CREATE PROCEDURE `AddRemoveStoryTag`(addtag bit(1), tagtype varchar(8), usrID bigint, usrIP int, sID bigint, taggID bigint, lon double, lat double)
BEGIN
IF addtag THEN
	IF tagtype='keyword' THEN
		insert ignore into StoryInfoKeywordTag (StoryID, InfoKeywordID, CreatedAt, UserID, IP)
		values (sID, taggID, utc_timestamp(), usrID, usrIP);
	ELSEIF tagtype='category' THEN
		insert ignore into StoryInfoCategoryTag (StoryID, InfoCategoryID, CreatedAt, UserID, IP)
		values (sID, taggID, utc_timestamp(), usrID, usrIP);
	ELSEIF tagtype='entity' THEN
		insert ignore into StoryInfoEntityTag (StoryID, InfoEntityID, CreatedAt, UserID, IP)
		values (sID, taggID, utc_timestamp(), usrID, usrIP);
	ELSEIF tagtype='location' THEN
		insert ignore into StoryLocationTag (StoryID, Longitude, Latitude, CreatedAt, UserID, IP)
		values (sID, lon, lat, utc_timestamp(), usrID, usrIP);
	END IF;
ELSE
	IF tagtype='keyword' THEN
		delete from StoryInfoKeywordTag where StoryID = sID and InfoKeywordID = taggID;
	ELSEIF tagtype='category' THEN
		delete from StoryInfoCategoryTag where StoryID = sID and InfoCategoryID = taggID;
	ELSEIF tagtype='entity' THEN
		delete from StoryInfoEntityTag where StoryID = sID and InfoEntityID = taggID;
	ELSEIF tagtype='location' THEN
		delete from StoryLocationTag where StoryID = sID and TagID = taggID;
	END IF;
END IF;

IF row_count()=1 THEN
    insert into StoryLog (IP, UserID, Timestamp, EventType, StoryAgeInSeconds, StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend, TagID)
    select usrIP, usrID, utc_timestamp(),
        case
            when addtag and tagtype='keyword' then 30
            when addtag and tagtype='category' then 31
            when addtag and tagtype='entity' then 32
            when addtag and tagtype='location' then 33
            when not addtag and tagtype='keyword' then 40
            when not addtag and tagtype='category' then 41
            when not addtag and tagtype='entity' then 42
            when not addtag and tagtype='location' then 43
        end,
        unix_timestamp(utc_timestamp())-unix_timestamp(StartTime),
        StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend,
        if(addtag and tagtype='location', last_insert_id(), taggID)
    from Story where StoryID=sID;
END IF;
END$$

CREATE PROCEDURE `HideShowStory`(sid bigint(20), hide bit(1), usrip int(11), usrid bigint(20))
BEGIN
update Story set IsHidden=hide where StoryID=sid;
insert into StoryLog (IP, UserID, Timestamp, EventType, StoryID, TweetCount, RetweetCount, UserCount, TopUserCount, Trend)
select usrip, usrid, utc_timestamp(), case when hide then 14 else 15 end, sid,
    TweetCount, RetweetCount, UserCount, TopUserCount, Trend
from Story where StoryID=sid;
END$$

CREATE FUNCTION `ScoreToIdf`(score double) RETURNS double
    DETERMINISTIC
return greatest(0, (1/(1+exp(-0.3*(score-10)))) * log((select 12*Value as D from Constants where Name='WordScore4dHigh')/score))$$

CREATE FUNCTION `ShortDate`(t datetime) RETURNS varchar(15) CHARSET latin1
    DETERMINISTIC
begin
set @utc = unix_timestamp(utc_date());
set @tt = unix_timestamp(date(t));
return case
    when @utc=@tt then cast(time(t) as char(5))
    else date_format(t, '%e %b %H:%i') end;
end$$

DELIMITER $$
CREATE FUNCTION `NaiveStemming`(term char(100)) RETURNS char(100) CHARSET utf8
begin
    declare retval char(100);
    declare tAfter char(100);
    declare tBefore char(100);

    if length(term)<4 or left(term,1)='#' 
        then set retval = term;
    else
        set tAfter = term;
        stemLoop : loop
            set tBefore = tAfter;
            set tAfter = case 
                when tAfter regexp '(ing)$' then left(tAfter,length(tAfter)-3)
                when tAfter regexp '(es|ed|ly)$' then left(tAfter,length(tAfter)-2)
                when tAfter regexp '(s|n)$' then left(tAfter,length(tAfter)-1) else tAfter end;
            if tAfter != tBefore 
                then iterate stemLoop; 
            end if;
            leave stemLoop;
        end loop stemLoop;
        set retval = tAfter;
    end if;
    return retval;
end$$
DELIMITER ;
