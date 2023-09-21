-- ------------------------------------------
-- procedure insert_participant
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `insert_participant`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `insert_participant` (IN WID CHAR(10), IN PSite CHAR(3))
BEGIN
INSERT INTO museqcapp.participants VALUES (WID, PSite);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_westonID
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `insert_westonID`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `insert_westonID` (IN WID CHAR(10))
BEGIN
INSERT INTO museqcapp.participants (westonID) VALUES (WID);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure westonID_exists
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `westonID_exists`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `westonID_exists` (IN WID CHAR(10))
BEGIN
SELECT EXISTS(SELECT * FROM museqcapp.participants WHERE westonID = WID);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure get_participantSite
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `get_participantSite`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `get_participantSite` (IN WID CHAR(10))
BEGIN
SELECT site 
FROM museqcapp.participants
WHERE westonID = WID;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure update_site
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `update_site`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `update_site` (IN WID CHAR(10), IN PSite CHAR(3))
BEGIN
UPDATE museqcapp.participants
SET site = PSite
WHERE westonID = WID;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_collectionBasicInfo
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `insert_collectionBasicInfo`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `insert_collectionBasicInfo` (IN WID CHAR(10), IN StartDT DATETIME,
	IN TimeOffset float, IN PodSerial CHAR(14), IN UploadDT DATETIME)
BEGIN
INSERT INTO museqcapp.collection (westonID, startDateTime, timeZoneOffset, podID, uploadDateTime, basicInfoAddedDateTime) 
VALUES (WID, StartDT, TimeOffset, PodSerial, UploadDT, NOW());
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure collectionBasicInfo_exists
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `collectionBasicInfo_exists`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `collectionBasicInfo_exists` (IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14))
BEGIN
SELECT EXISTS(
	SELECT *
	FROM museqcapp.collection 
	WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial
);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure update_edfPath
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `update_edfPath`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `update_edfPath` (IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14), IN edfFullPath VARCHAR(128))
BEGIN
UPDATE museqcapp.collection
SET edfPath = edfFullPath
WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure edf_exists
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `edf_exists`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `edf_exists` (IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14))
BEGIN
SELECT EXISTS(
	SELECT *
	FROM museqcapp.collection 
	WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial AND edfPath IS NOT NULL AND edfPath != ""
);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure get_unprocessed_edfPaths
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `get_unprocessed_edfPaths`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `get_unprocessed_edfPaths` ()
BEGIN

SELECT edfPath
FROM museqcapp.collection 
WHERE outputsAddedDateTime IS NULL AND edfPath IS NOT Null AND edfPath != "" AND processingProblem IS NULL;

END$$

DELIMITER ;

-- ------------------------------------------
-- procedure jpg_exists
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `jpg_exists`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `jpg_exists` (IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14))
BEGIN
SELECT EXISTS(
	SELECT *
	FROM museqcapp.collection 
	WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial AND jpgPath IS NOT NULL AND jpgPath != ""
);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_qualityOutputs
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `insert_qualityOutputs`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `insert_qualityOutputs` (
	IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14), 
	IN Dur DOUBLE, IN Ch1 DOUBLE, IN Ch2 DOUBLE, IN Ch3 DOUBLE, IN Ch4 DOUBLE,
    IN Ch12 DOUBLE, IN Ch13 DOUBLE, IN Ch43 DOUBLE, IN Ch42 DOUBLE,
    IN FAny DOUBLE, IN FBoth DOUBLE, IN TAny DOUBLE, IN TBoth DOUBLE,
    IN FtAny DOUBLE, IN EegAny DOUBLE, IN EegAll DOUBLE,
	IN JpgPath VARCHAR(128), IN IsTest BOOLEAN, 
    IN DurProb BOOLEAN, IN QualityProb BOOLEAN, IN QualityVersion INT
)
BEGIN
DECLARE cid INT unsigned;  
SET cid = (SELECT collectionID FROM collection WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial);

UPDATE museqcapp.collection
SET 
jpgPath = JpgPath, 
isTest = IsTest, 
hasDurationProblem = DurProb, 
hasQualityProblem = QualityProb, 
outputsAddedDateTime = NOW(), 
museQualityVersion = QualityVersion,
processingProblem = 0
WHERE collectionID = cid; 

INSERT INTO museqcapp.qcstats (collectionID, duration, eegch1, eegch2, eegch3, eegch4, 
	eeg_ch1_eeg_ch2, eeg_ch1_eeg_ch3, eeg_ch4_eeg_ch3, eeg_ch4_eeg_ch2, 
    fany, fboth, tany, tboth, ftany, eegany, eegall) 
VALUES (cid, Dur, Ch1, Ch2, Ch3, Ch4, Ch12, Ch13, Ch43, Ch42, 
	FAny, FBoth, TAny, TBoth, FtAny, EegAny, EegAll);

END$$

DELIMITER ;

-- ------------------------------------------
-- procedure get_lastDateTimeDownloaded
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `get_lastDateTimeDownloaded`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `get_lastDateTimeDownloaded` ()
BEGIN

SELECT IF(
	(SELECT count(collectionID) FROM museqcapp.collection WHERE outputsAddedDateTime IS NULL AND edfPath IS NULL) > 0,
	(SELECT MIN(uploadDateTime) FROM museqcapp.collection WHERE outputsAddedDateTime IS NULL AND edfPath IS NULL),
	(SELECT MAX(uploadDateTime) FROM museqcapp.collection)
);

END$$

DELIMITER ;

-- ------------------------------------------
-- procedure update_problemProcessing
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `update_problemProcessing`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `update_problemProcessing` (IN WID CHAR(10), IN StartDT DATETIME, IN PodSerial CHAR(14), IN Problem tinyint)
BEGIN
UPDATE museqcapp.collection
SET processingProblem = Problem
WHERE westonID = WID AND startDateTime = StartDT AND podID = PodSerial;
END$$

-- ------------------------------------------
-- procedure get_processedEdfFileList
-- ------------------------------------------
USE `museqcapp`;
DROP procedure IF EXISTS `get_processedEdfList`;

DELIMITER $$
USE `museqcapp`$$
CREATE PROCEDURE `get_processedEdfFileList` ()
BEGIN
SELECT c.westonID, c.podID, c.startDateTime
FROM museqcapp.collection as c
WHERE processingProblem IS NOT NULL;
END$$

DELIMITER ;