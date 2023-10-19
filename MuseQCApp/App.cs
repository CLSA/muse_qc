using Microsoft.Extensions.Logging;
using MuseQCApp.Helpers;
using MuseQCApp.Interfaces;
using MuseQCApp.Logic;
using MuseQCApp.Models;
using MuseQCDBAccess.Data;
using MuseQCDBAccess.Models;

namespace MuseQCApp;

public class App
{
    #region private properties

    /// <summary>
    /// Helper to access configuration settings
    /// </summary>
    private ConfigHelper ConfigHelper { get; init; }

    /// <summary>
    /// The logger to use
    /// </summary>
    private ILogger Logging { get; init; }

    /// <summary>
    /// A module to use to interact with the google bucket
    /// </summary>
    private IGoogleBucket Bucket {  get; init; }

    /// <summary>
    /// A module for running the muse quality checks
    /// </summary>
    private IMuseQualityRunner QualityRunner { get; init; }

    /// <summary>
    /// A module for determining is muse data quality is acceptable
    /// </summary>
    private IMuseQualityDecisions MuseQuality { get; init; }

    /// <summary>
    /// A writer for writing report csvs
    /// </summary>
    private ReportCsvWriter ReportWriter { get; init; }

    /// <summary>
    /// Helper to interact with DB
    /// </summary>
    private DbHelpers DbMethods { get; init; }

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="configHelper">Helper to access configuration settings</param>
    /// <param name="logging">The logger to use</param>
    /// <param name="bucket">A module to use to interact with the google bucket</param>
    /// <param name="qualityRunner">A module for running the muse quality checks</param>
    /// <param name="museQuality">A module for determining is muse data quality is acceptable</param>
    /// <param name="qualityReport">A module for creating muse quality reports</param>
    public App(ConfigHelper configHelper, ILogger logging, IGoogleBucket bucket,IMuseQualityRunner qualityRunner, 
        IMuseQualityDecisions museQuality, MysqlDBData db)
    {
        ConfigHelper = configHelper;
        Logging = logging;
        Bucket = bucket;
        QualityRunner = qualityRunner;
        MuseQuality = museQuality;
        DbMethods = new(db, logging);
        ReportWriter = new(logging);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Runs the quality checks application process
    /// </summary>
    public void Run(int maxFilesToDownloadAtOnce = 10, int maxFilesToDownload = -1)
    {
        string appName = AppDomain.CurrentDomain.FriendlyName;
        Logging.LogInformation($"{appName} started running");

        List<ParticipantCollectionsQualityModel> participants = DbMethods.GetParticipantReportData();
        string? reportFolderPath = ConfigHelper.GetQualityReportCsvFolderPath();
        if (Directory.Exists(reportFolderPath))
        {
            ReportWriter.CreateReportCsvs(participants, reportFolderPath);
        }
        else
        {
            Logging.LogWarning($"Report folder not found. Path: {reportFolderPath}");
        }
        
        // Get list of files on google bucket
        List<GBDownloadInfoModel> pathsOnBucket = Bucket.GetFilePaths();

        // Select all files that need to be downloaded
        List<GBDownloadInfoModel> pathsToDownload = DecideFilesToDownload(pathsOnBucket);

        // set numFilesToDownload to the number of available files if:
        // 1. a value was not provided for maxFilesToDownload
        // 2. the value provided for maxFilesToDownload is higher than the number of available files
        // Otherwise download and process a number of files equal to maxFilesToDownload
        int totalNumFilesToDownload = ((pathsToDownload.Count < maxFilesToDownload) || (maxFilesToDownload == -1)) 
            ? pathsToDownload.Count : maxFilesToDownload;

        int filesDownloaded = 0;
        while ( filesDownloaded < totalNumFilesToDownload)
        {
            // decide how many files to download this loop execution
            // while ensuring the max total number is not exceeded
            int filesLeftToDownload = totalNumFilesToDownload - filesDownloaded;
            int numFilesToDownloadThisExecution = filesLeftToDownload > maxFilesToDownloadAtOnce
                ? maxFilesToDownloadAtOnce : filesLeftToDownload;
            
            // Download files that have not had quality checks run
            // and are not currently downloaded
            DownloadFiles(pathsToDownload, numFilesToDownloadThisExecution);

            // Run quality checks
            RunQualityChecks();

            // increment the number of files that have been downloaded
            filesDownloaded+= numFilesToDownloadThisExecution;
        }

        // Create reports
        // TODO: Implement
        UpdateParticipantSiteInfo();

        Logging.LogInformation($"{appName} done running");
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Download all files
    /// </summary>
    /// <param name="pathsOnBucket">The paths to all files that exist in the google bucket</param>
    /// <param name="maxFilesToDownload">The maximum number of files that should be downloaded</param>
    private void DownloadFiles(List<GBDownloadInfoModel> pathsToDownload, int maxFilesToDownload = 10)
    {
        // Reduce the amount of files to download to a max specified amount
        List<GBDownloadInfoModel> selectedPathsToDownload = SelectXFiles(pathsToDownload, maxFilesToDownload); 
        
        // Download files and update DB with paths
        string? edfStorageFolderPath = ConfigHelper.GetEdfStorageFolderPath();
        if (string.IsNullOrEmpty(edfStorageFolderPath) == false)
        {
            List<GBDownloadInfoModel> filesDownloadedSuccessfully = Bucket.DownloadFiles(selectedPathsToDownload, edfStorageFolderPath);
            foreach(GBDownloadInfoModel fileDownloaded in filesDownloadedSuccessfully)
            {
                pathsToDownload.Remove(fileDownloaded);
            }
            DbMethods.UpdateDbWithDownloadedFilePaths(filesDownloadedSuccessfully, edfStorageFolderPath); 
        }
        else
        {
            Logging.LogInformation($"No edf storage folder found in configuration");
        }
    }

    /// <summary>
    /// Runs quality checks on files that require quality checks
    /// </summary>
    private void RunQualityChecks()
    {
        // Get output and jpg folder paths (return if either is null)
        string? outDirPath = ConfigHelper.GetOutputStorageFolderPath();
        string? jpgDirPath = ConfigHelper.GetJpgStorageFolderPath();
        if (outDirPath is null || jpgDirPath is null) return;

        // Format out dir path and Create out folder if it does not exist
        string outDir = $"{outDirPath.Replace("\\", "/")}";
        if (Directory.Exists(outDir) == false) 
        { 
            Directory.CreateDirectory(outDir);
        }

        // Determine files that need to have quality checks run
        IEnumerable<string> edfFiles = DbMethods.GetEdfFilesThatNeedQualityChecks();

        Logging.LogInformation($"Found {edfFiles.Count()} files that need quality checks run");

        // Run quality checks
        foreach (string edf in edfFiles)
        {
            // Log startime and file size of file that is starting to run
            DateTime startTime = DateTime.Now;
            long fileSize = new FileInfo(edf).Length;
            Logging.LogInformation($"Started: {edf} at {startTime}. FileSize: {fileSize}");

            // Run quality check R script 
            MuseQualityOutputPathsModel? outputPaths = QualityRunner.RunMuseQualityCheck(edf, outDir);

            // Log error if any of the output files do not exist
            if (outputPaths is null || File.Exists(outputPaths.JpgPath) == false
                || File.Exists(outputPaths.CsvPath) == false || File.Exists(outputPaths.EdfPath) == false)
            {
                Logging.LogError($"Not all output files were created by the quality script for input file: {edf}");
                UpdateEdfThatFailedProcessing(edf);
                continue;
            }

            // Update Muse quality information in DB
            MuseQualityResultsModel? resultsPackage = CreateMuseQualityResultsModel(outputPaths, jpgDirPath);
            bool runSuccessfully = false;
            if(resultsPackage is not null)
            {
                runSuccessfully = DbMethods.UpdateDbMuseQuality(resultsPackage);
            }
            

            // Log error if unable to update the DB
            if (runSuccessfully == false)
            {
                Logging.LogError($"Unable to update db with output paths for input file: {edf}\n" +
                    $"\tJpg: {outputPaths.JpgPath}\n\tCsv: {outputPaths.CsvPath}\n\tEdf: {outputPaths.EdfPath}\n");
                continue;
            }

            // Log end time when completed running quality check for participant
            DateTime endTime = DateTime.Now;
            Logging.LogInformation($"Completed: {edf} at {endTime}. Duration: {endTime-startTime}");
            
            // Delete unneeded files
            File.Delete(outputPaths.CsvPath);
            File.Delete(outputPaths.EdfPath);
            File.Delete(edf);
        }
    }

    /// <summary>
    /// Selects files from oldest upload date to newest
    /// </summary>
    /// <param name="files">The information of files that can be selected</param>
    /// <param name="numToSelect">The number of files to be selected</param>
    /// <returns>Information for the selected files</returns>
    private List<GBDownloadInfoModel> SelectXFiles(List<GBDownloadInfoModel> files, int numToSelect)
    {
        files = files.OrderBy(x => x.UploadDateTime).ToList();
        int maxINt = numToSelect < files.Count ? numToSelect : files.Count;
        List<GBDownloadInfoModel> selectedPathsToDownload = new();
        for (int i = 0; i < maxINt; i++)
        {
            selectedPathsToDownload.Add(files[i]);
        }
        return selectedPathsToDownload;
    }

    /// <summary>
    /// Creates a MuseQualityResultsModel with all of the information required by the data base
    /// </summary>
    /// <param name="outputPaths">The paths to the files output by the Muse Quality R script</param>
    /// <param name="jpgDirPath">The path to the directory where jpg files should be stored</param>
    /// <returns>a MuseQualityResultsModel with all of the information required by the data base</returns>
    private MuseQualityResultsModel? CreateMuseQualityResultsModel(MuseQualityOutputPathsModel outputPaths, string jpgDirPath)
    {
        // Read data from output csv
        QCStatsModel? qcStats = QualityRunner.ReadOutputCsv(outputPaths.CsvPath);

        // Interpret values from jpg filename
        // NOTE: Assumes jpg filename is the same as the google bucket name
        string fileName = Path.GetFileNameWithoutExtension(outputPaths.JpgPath);
        string? westonId = MuseGBFileName.GetWestonID(fileName);
        string? podSerial = MuseGBFileName.GetPodID(fileName);
        DateTime? startDate = MuseGBFileName.GetStartDateTime(fileName);

        // Log error if any of the data is missing
        if (qcStats is null || westonId is null || podSerial is null || startDate is null)
        {
            Logging.LogError($"Unable to read all info. qcStats: {(qcStats is null ? "NULL" : "Correct")}" +
                $" westonID: {westonId} podSerial: {podSerial} startDate: {startDate}");
            return null;
        }

        // Interpret the results
        bool isTest = MuseQuality.IsTest(qcStats);
        bool durProblem = MuseQuality.HasDurationProblem(qcStats);
        bool qualityProblem = MuseQuality.HasQualityProblem(qcStats);

        // Move jpg file to jpg specific folder
        string newJpgPath = Path.Combine(jpgDirPath, Path.GetFileName(outputPaths.JpgPath));
        File.Move(outputPaths.JpgPath, newJpgPath);

        return new MuseQualityResultsModel(qcStats, fileName, westonId, podSerial, startDate.Value, isTest, durProblem, qualityProblem, MuseQuality.GetVersionNumber(), newJpgPath);
    }

    /// <summary>
    /// Creates a list of the files that are not currently downloaded that need to have analysis run
    /// </summary>
    /// <param name="downloadableFiles">A list of information on all edf files that exist in the google bucket</param>
    /// <returns>A list of the files that are not currently downloaded that need to have analysis run</returns>
    public List<GBDownloadInfoModel> DecideFilesToDownload(List<GBDownloadInfoModel> downloadableFiles)
    {
        // Get the info for all of the previously processed files
        var processedFileList = DbMethods.GetProcessedEdfList();

        // Check each file that can be downloaded and select those that need to be downloaded
        List<GBDownloadInfoModel> filesToDownload = new();
        foreach (GBDownloadInfoModel gbInfo in downloadableFiles)
        {
            try
            {
                // Ignore if:
                // 1. Missing information
                // 2. The id is not a ww id
                // 3. The file size is less than 1mb
                // 4. The file was previously processed
                if (gbInfo.NoNullValues == false
                    || gbInfo.WestonID.ToLower().StartsWith("ww") == false
                    || gbInfo.LessThan1mb
                    || ProcessedAlready(gbInfo, processedFileList))
                {
                    continue;
                }

                bool dbUpToDate = DbMethods.InsertGBDataIntoDb(gbInfo);
                if (dbUpToDate)
                {
                    filesToDownload.Add(gbInfo);
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"Error while evaluating {gbInfo.FileNameWithExtension}. Msg: {ex.Message}");
            }
        }
         return filesToDownload;
    }

    /// <summary>
    /// Checks if a file has been processed already
    /// </summary>
    /// <param name="gbInfo">The info to check against the list</param>
    /// <param name="processedFileList">A list of all the files that have had data processed and stored in the DB</param>
    /// <returns>True if the file</returns>
    private bool ProcessedAlready(GBDownloadInfoModel gbInfo, List<CollectionDataPrimaryKeyModel> processedFileList)
    {
        if(gbInfo.NoNullValues)
        {
            string westonID = gbInfo.WestonID.ToLower();
            foreach (CollectionDataPrimaryKeyModel processedFile in processedFileList)
            {
                if (westonID.Equals(processedFile.westonID.ToLower())
                    && gbInfo.PodID.Equals(processedFile.podID)
                    && gbInfo.CollectionDateTime.Equals(processedFile.startDateTime))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Makes the appropriate updates required for any edf file that fails processing
    /// 1. Moves the file to a new sub folder for files that failed processing 
    ///     NOTE: Assumes the old edf path is stored in the db
    /// 2. Updates the DB with the new path
    /// 3. updates DB to set processing problem flag
    /// </summary>
    /// <param name="edfPath">The path to where the edf file is located</param>
    private void UpdateEdfThatFailedProcessing(string edfPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(edfPath);
        string? westonId = MuseGBFileName.GetWestonID(fileName);
        string? podSerial = MuseGBFileName.GetPodID(fileName);
        DateTime? startDate = MuseGBFileName.GetStartDateTime(fileName);

        if(westonId is null || podSerial is null || startDate is null)
        {
            Logging.LogError($"Could not obtain one or more of the following, WestonID, Pod serial number or start date from path: {edfPath}");
            return;
        }

        string? edfproblemDir = ConfigHelper.GetEdfProblemStorageSubFolderPath();
        if (edfproblemDir is null)
        {
            return;
        }

        string updatedEdfPath = Path.Combine(edfproblemDir, $"{fileName}.edf");

        // Move file to folder specific for files with problems
        File.Move(edfPath, updatedEdfPath);

        DbMethods.UpdateEdfFailedProcessing(westonId, podSerial, startDate.Value, updatedEdfPath);
        Logging.LogInformation($"Updated db with processing problem and new path. Old path: {edfPath} New path: {updatedEdfPath}");
    }

    /// <summary>
    /// Update the participant site info for any participants that do not have it entered yet
    /// </summary>
    private void UpdateParticipantSiteInfo()
    {
        string? lookupTablePath = ConfigHelper.GetSiteLookupTableCsvPath();
        if (lookupTablePath != null)
        {
            List<ParticipantModel> participants = SiteLookupTable.ReadSiteLookupTableCsv(lookupTablePath, Logging);
            DbMethods.UpdateParticipantSites(participants);
        }
    }

    #endregion
}
