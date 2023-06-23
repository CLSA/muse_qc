-- ------------------------------------------
-- procedure insert_westonID
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_westonID`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_westonID` (IN wid CHAR(10))
BEGIN
INSERT INTO museqc.participants (westonID) VALUES (wid);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure westonID_exists
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `westonID_exists`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `westonID_exists` (IN wid CHAR(10))
BEGIN
SELECT EXISTS(SELECT * FROM museqc.participants WHERE westonID = wid);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure get_participantSite
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `get_participantSite`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `get_participantSite` (IN wid CHAR(10))
BEGIN
SELECT site 
FROM museqc.participants
WHERE westonID = wid;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure update_site
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `update_site`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `update_site` (IN wid CHAR(10), IN newSite CHAR(10))
BEGIN
UPDATE museqc.participants
SET site = newSite
WHERE westonID = wid;
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure insert_collectionBasicInfo
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `insert_collectionBasicInfo`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `insert_collectionBasicInfo` (IN wid CHAR(10), IN start_date DATETIME,
	IN timezone_offset float, IN pod_id CHAR(14), IN upload_date DATE)
BEGIN
INSERT INTO museqc.collection (westonID, startDate, timeZoneOffset, podID, uploadDate) 
VALUES (wid, start_date, timezone_offset, pod_id, upload_date);
END$$

DELIMITER ;

-- ------------------------------------------
-- procedure collectionBasicInfo_exists
-- ------------------------------------------
USE `museqc`;
DROP procedure IF EXISTS `collectionBasicInfo_exists`;

DELIMITER $$
USE `museqc`$$
CREATE PROCEDURE `collectionBasicInfo_exists` (IN wid CHAR(10), IN start_date DATETIME, IN pod_id CHAR(14))
BEGIN
SELECT EXISTS(
	SELECT *
	FROM museqc.collection 
	WHERE westonID = wid AND startDate = start_date AND podID = pod_id
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
	IN wid CHAR(10), IN start_date DATETIME, IN pod_id CHAR(14), 
	IN dur DOUBLE, IN ch1 DOUBLE, IN ch2 DOUBLE, IN ch3 DOUBLE, IN ch4 DOUBLE,
    IN ch12 DOUBLE, IN ch13 DOUBLE, IN ch43 DOUBLE, IN ch42 DOUBLE,
    IN f_any DOUBLE, IN f_both DOUBLE, IN t_any DOUBLE, IN t_both DOUBLE,
    IN ft_any DOUBLE, IN eeg_any DOUBLE, IN eeg_all DOUBLE,
	IN jpg_path VARCHAR(256), IN is_real_data BOOLEAN, IN has_problem BOOLEAN
)
BEGIN
SELECT @cid := collectionID FROM collection WHERE westonID = wid AND startDate = start_date AND podID = pod_id;

UPDATE collection
SET jpgPath = jpg_path, isRealDay = is_real_data, hasProblem = has_problem
WHERE collectionID = @cid; 

INSERT INTO museqc.qcstats (collectionID, duration, eegch1, eegch2, eegch3, eegch4, 
	eeg_ch1_eeg_ch2, eeg_ch1_eeg_ch3, eeg_ch4_eeg_ch3, eeg_ch4_eeg_ch2, 
    fany, fboth, tany, tboth, ftany, eegany, eegall) 
VALUES (@cid, dur, ch1, ch2, ch3, ch4, ch12, ch13, ch43, ch42, 
	f_any, f_both, t_any, t_both, ft_any, eeg_any, eeg_all);

END$$

DELIMITER ;