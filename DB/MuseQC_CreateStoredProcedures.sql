-- ------------------------------------------
-- procedure insert_participant
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_participant`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_participant` (IN WID CHAR(10), IN PSite CHAR(3))
BEGIN
INSERT INTO museqc.participants VALUES (WID, PSite);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_westonID
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_westonID`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_westonID` (IN WID CHAR(10))
BEGIN
INSERT INTO museqc.participants (westonID) VALUES (WID);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure westonID_exists
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `westonID_exists`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `westonID_exists` (IN WID CHAR(10))
BEGIN
SELECT EXISTS(SELECT * FROM museqc.participants WHERE westonID = WID);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure get_participantSite
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `get_participantSite`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `get_participantSite` (IN WID CHAR(10))
BEGIN
SELECT site 
FROM museqc.participants
WHERE westonID = WID;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure update_site
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `update_site`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `update_site` (IN WID CHAR(10), IN PSite CHAR(3))
BEGIN
UPDATE museqc.participants
SET site = PSite
WHERE westonID = WID;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_collectionBasicInfo
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_collectionBasicInfo`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_collectionBasicInfo` (IN WID CHAR(10), IN StartDateTime DATETIME,
	IN TimezoneOffset float, IN PodID CHAR(14), IN UploadDate DATE)
BEGIN
INSERT INTO museqc.collection (westonID, startDate, timeZoneOffset, podID, uploadDate) 
VALUES (WID, StartDateTime, TimezoneOffset, PodID, UploadDate);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure collectionBasicInfo_exists
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `collectionBasicInfo_exists`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `collectionBasicInfo_exists` (IN WID CHAR(10), IN StartDateTime DATETIME, IN PodID CHAR(14))
BEGIN
SELECT EXISTS(
	SELECT *
	FROM museqc.collection 
	WHERE westonID = WID AND startDate = StartDateTime AND podID = PodID
);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_qualityOutputs
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_qualityOutputs`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_qualityOutputs` (
	IN WID CHAR(10), IN StartDateTime DATETIME, IN PodID CHAR(14), 
	IN Dur DOUBLE, IN Ch1 DOUBLE, IN Ch2 DOUBLE, IN Ch3 DOUBLE, IN Ch4 DOUBLE,
    IN Ch12 DOUBLE, IN Ch13 DOUBLE, IN Ch43 DOUBLE, IN Ch42 DOUBLE,
    IN FAny DOUBLE, IN FBoth DOUBLE, IN TAny DOUBLE, IN TBoth DOUBLE,
    IN FtAny DOUBLE, IN EegAny DOUBLE, IN EegAll DOUBLE,
	IN JpgPath VARCHAR(256), IN IsRealData BOOLEAN, IN HasProblem BOOLEAN
)
BEGIN
DECLARE cid INT unsigned;  
SET cid = (SELECT collectionID FROM collection WHERE westonID = WID AND startDate = StartDateTime AND podID = PodID);

UPDATE collection
SET jpgPath = JpgPath, isRealDay = IsRealData, hasProblem = HasProblem
WHERE collectionID = cid; 

INSERT INTO museqc.qcstats (collectionID, duration, eegch1, eegch2, eegch3, eegch4, 
	eeg_ch1_eeg_ch2, eeg_ch1_eeg_ch3, eeg_ch4_eeg_ch3, eeg_ch4_eeg_ch2, 
    fany, fboth, tany, tboth, ftany, eegany, eegall) 
VALUES (cid, Dur, Ch1, Ch2, Ch3, Ch4, Ch12, Ch13, Ch43, Ch42, 
	FAny, FBoth, TAny, TBoth, FtAny, EegAny, EegAll);

END$$

DELIMITER ;