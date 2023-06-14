using MuseQCApp.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for making descions that require knowledge of file locations
/// </summary>
public interface IFileLocations
{
    /// <summary>
    /// Decides and keeps entries in the dict which need edf files to be downloaded. Removes any 
    /// entries where there is already either edf files or if their is a complete set of 
    /// output files (.jpg and .csv)
    /// </summary>
    /// <param name="possibleFilesDict">The possible files that can be selected for download</param>
    public void DecideFilesToDownload(Dictionary<string, object> possibleFilesDict);

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
