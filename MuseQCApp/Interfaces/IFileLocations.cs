using MuseQCApp.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for making decisions that require knowledge of file locations
/// </summary>
public interface IFileLocations
{
    /// <summary>
    /// Decides and which edf files to be downloaded. Selects all files as long as they do 
    /// not meet either of the following 2 criteria 
    /// 1. The edf file currently exists in the file system
    /// 2. There is already a complete set of output data for the edf file (ie. quality script has been run)
    /// </summary>
    /// <param name="downloadableFiles">The files that can be selected for download</param>
    /// <returns>The <see cref="GBDownloadInfoModel"/> of files that should be downloaded</returns>
    public List<GBDownloadInfoModel> DecideFilesToDownload(List<GBDownloadInfoModel> downloadableFiles);

    /// <summary>
    /// Gets the full file paths for each edf file that needs a quality check run
    /// </summary>
    /// <returns>A list of full file paths for edf files that need quality checks run</returns>
    public List<string> NeedsQualityCheck();

    /// <summary>
    /// Gets information to be used to create a report for a certain time range
    /// </summary>
    /// <param name="StartDate">The starting date time of data to be include</param>
    /// <param name="EndDate">The end date time of data to include</param>
    /// <returns>The information to be used to create a report</returns>
    public ReportInfoModel GetReportInfo(DateTime StartDate, DateTime EndDate);
}
