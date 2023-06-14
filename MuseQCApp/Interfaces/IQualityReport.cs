using MuseQCApp.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for methods to build reports
/// </summary>
public interface IQualityReport
{
    /// <summary>
    /// Creates reports containing the info passed in
    /// </summary>
    /// <param name="reportInfo">The report info to use when building the report</param>
    /// <param name="outputPath">The path to output the reports to</param>
    /// <returns>True if all of the expected reports are created, false otherwise</returns>
    public bool Create(ReportInfoModel reportInfo, string outputPath);
}
