using Microsoft.Extensions.Logging;
using MuseQCApp.Models;
using MuseQCDBAccess.Data;

namespace MuseQCApp.Helpers;
public class DbHelpers
{

    #region Private properties

    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    /// <summary>
    /// Helper to run stored procedures in database
    /// </summary>
    private MysqlDBData Db { get; init; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="db"></param>
    /// <param name="logging"></param>
    public DbHelpers(MysqlDBData db, ILogger logging)
    {
        Db = db;
        Logging = logging;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Updates the database by:
    /// 1. adding qc stats
    /// 2. adding jpg path
    /// 3. adding interpreted results
    /// 4. removing edf path
    /// </summary>
    /// <param name="outputPaths">The paths to all files created by the quality script</param>
    /// <param name="jpgDirPath">The new jpg directory where the jpg file should be moved to</param>
    /// <returns>True if completed successfully, and false otherwise</returns>
    public bool UpdateDbMuseQuality(MuseQualityResultsModel results)
    {
        // Update db with jpg path and data (qc stats, filename inferred data, and interpreted results)
        Db.Collection.InsertQualityOutputs(results.WestonId, results.StartDate, results.PodSerial,
            results.QcStats, results.NewJpgPath, results.RealData, results.DurProblem, results.QualityProblem, results.MuseQualityVersion);

        // Remove edf path from db
        bool edfExists = Db.Collection.EdfExists(results.WestonId, results.StartDate, results.PodSerial).Result.First();
        string deviceInfoStr = $"Weston ID: {results.WestonId} Pod Serial: {results.PodSerial} Start Date: {results.StartDate}";
        if (edfExists)
        {
            Db.Collection.UpdateEdfPath(results.WestonId, results.StartDate, results.PodSerial, "");
            Logging.LogInformation($"Removed Edf file path from db for a collection with the following details. {deviceInfoStr}");
        }
        else
        {
            Logging.LogWarning($"No Edf file no was found in db for a collection with the following details. {deviceInfoStr}");
        }
        return true;
    }

    /// <summary>
    /// Adds the paths that were downloaded to the database
    /// </summary>
    /// <param name="pathsThatWereDownload">The information for files that were downloaded</param>
    /// <param name="edfStorageFolderPath">The path to were edf files are stored</param>
    public void UpdateDbWithDownloadedFilePaths(List<GBDownloadInfoModel> pathsThatWereDownload, string edfStorageFolderPath)
    {
        foreach (GBDownloadInfoModel gbInfo in pathsThatWereDownload)
        {
            string fullFilePath = gbInfo.GetDownloadFilePath(edfStorageFolderPath);
            if (gbInfo.NoNullValues == false) continue;
            // Values below will never be null due to check above
            Db.Collection.UpdateEdfPath(gbInfo.WestonID, gbInfo.CollectionDateTime.Value, gbInfo.PodID, fullFilePath).Wait();
        }
    }

    /// <summary>
    /// Get the upload date time of the last file downloaded
    /// </summary>
    /// <returns>The upload date time of the last file downloaded, or Null if there has no been any files downloaded</returns>
    public DateTime? GetUploadDTLastFileDownloaded()
    {
        DateTime? lastTimeDownloaded = Db.ConfigVals.GetLastDateDownloaded().Result.First();
        return lastTimeDownloaded;
    }

    /// <summary>
    /// Insert data from the google bucket info into the database if it doesn't exist already
    /// </summary>
    /// <param name="gbInfo">The google bucket info to use</param>
    /// <returns>True if there were not no problem, false if an error occurs</returns>
    public bool InsertGBDataIntoDb(GBDownloadInfoModel gbInfo)
    {
        // Do not attempt to enter values in db if any data is null (aka missing)
        if (gbInfo.NoNullValues == false) return false;
        string westonID = gbInfo.WestonID ?? "";
        DateTime startDateTime = gbInfo.CollectionDateTime ?? DateTime.Now;
        string podID = gbInfo.PodID ?? "";

        // Insert weston id into database if it doesn't exist
        // return false if there is a problem inserting into the db
        if (InsertWestonID(westonID) == false) return false;

        // Insert collection basic info into database if it doesn't exist
        // return true if there are no problems,
        // otherwise false if any errors occur
        return InsertCollectionBasicInfo(westonID, startDateTime, podID, gbInfo);
    }

    /// <summary>
    /// Gets all edf files that need quality checks
    /// </summary>
    /// <returns>a list of all edf files that need quality checks</returns>
    public IEnumerable<string> GetEdfFilesThatNeedQualityChecks()
    {
        return Db.Collection.GetUnprocessedEdfPaths().Result;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Insert the weston id into the db if it does not exists in the db
    /// </summary>
    /// <param name="westonID">The weston id to insert</param>
    /// <returns>True if there are no problems, false if any errors occur</returns>
    private bool InsertWestonID(string westonID)
    {
        // Add weston ID to DB if it does not already exist
        bool westonIdExists = Db.Participant.WestonIDExists(westonID).Result.First();
        if (westonIdExists == false)
        {
            bool completed = Db.Participant.InsertWestonID(westonID).Wait(5000);
            if (completed == false)
            {
                Logging.LogError($"Unable to insert WestonID: {westonID} into DB");
                return false;
            }
            Logging.LogInformation($"Inserted WestonID: {westonID} into DB");
        }

        return true;
    }

    private bool InsertCollectionBasicInfo(string westonID, DateTime startDateTime, string podID, GBDownloadInfoModel gbInfo)
    {
        // Log error if the pod id is not of length 14
        if (podID.Length != 14)
        {
            Logging.LogError($"Incorrectly formatted pod id. Expected length: 14 Actual: {podID} ({podID.Length})");
            return false;
        }

        // Add collection basic info to Db if it does not already exist
        bool collectionBasicInfoExists = Db.Collection.CollectionExists(westonID, startDateTime, podID).Result.First();
        if (collectionBasicInfoExists == false)
        {
            // None Will not be null because No Null check already preformed above
            float offset = gbInfo.TimeZoneOffset ?? 0;

            bool completed = Db.Collection.InsertBasicInfo(westonID, startDateTime, offset, podID, gbInfo.UploadDateTime).Wait(5000);
            if (completed == false)
            {
                Logging.LogError($"Unable to insert Collection Basic info into DB " +
                    $"WID: {westonID} Pod id: {podID} Offset: {offset} start: {startDateTime} Upload: {gbInfo.UploadDateTime}");
                return false;
            }
            Logging.LogInformation($"Inserted Basic info into DB at {DateTime.Now}. WestonID: {westonID} Start: {startDateTime} ({offset}) Pod: {podID} Upload: {gbInfo.UploadDateTime}");
        }
        return true;
    }

#endregion  
}
