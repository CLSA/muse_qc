CREATE SCHEMA `museqcapp` ;

CREATE TABLE IF NOT EXISTS museqcapp.`participants` (
  `westonID` char(10) NOT NULL,
  `site` char(3) DEFAULT NULL,
  PRIMARY KEY (`westonID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS  museqcapp.`collection` (
  `collectionID` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `westonID` char(10) NOT NULL,
  `startDateTime` datetime NOT NULL,
  `timeZoneOffset` float NOT NULL,
  `podID` char(14) NOT NULL,
  `uploadDateTime` datetime NOT NULL,
  `edfPath` varchar(128) DEFAULT NULL,
  `processingProblem` tinyint DEFAULT NULL,
  `basicInfoAddedDateTime` datetime DEFAULT NULL,
  `outputsAddedDateTime` datetime DEFAULT NULL,
  `jpgPath` varchar(128) DEFAULT NULL,
  `isTest` tinyint DEFAULT NULL,
  `hasDurationProblem` tinyint DEFAULT NULL,
  `hasQualityProblem` tinyint DEFAULT NULL,
  `museQualityVersion` int UNSIGNED DEFAULT NULL,
  PRIMARY KEY (`collectionID`),
  UNIQUE (`westonID`, `startDateTime`, `podID`),
  KEY `WestonID_FK_idx` (`westonID`),
  CONSTRAINT `WestonID_FK` FOREIGN KEY (`westonID`) REFERENCES `participants` (`westonID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS  museqcapp.`qcstats` (
  `qcid` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `collectionID` int UNSIGNED NOT NULL,
  `duration` double NOT NULL,
  `eegch1` double NOT NULL,
  `eegch2` double NOT NULL,
  `eegch3` double NOT NULL,
  `eegch4` double NOT NULL,
  `eeg_ch1_eeg_ch2` double NOT NULL,
  `eeg_ch1_eeg_ch3` double NOT NULL,
  `eeg_ch4_eeg_ch3` double NOT NULL,
  `eeg_ch4_eeg_ch2` double NOT NULL,
  `fany` double NOT NULL,
  `fboth` double NOT NULL,
  `tany` double NOT NULL,
  `tboth` double NOT NULL,
  `ftany` double NOT NULL,
  `eegany` double NOT NULL,
  `eegall` double NOT NULL,
  PRIMARY KEY (`qcid`),
  KEY `collectionID_FK_idx` (`collectionID`),
  CONSTRAINT `collectionID_FK` FOREIGN KEY (`collectionID`) REFERENCES `collection` (`collectionID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;