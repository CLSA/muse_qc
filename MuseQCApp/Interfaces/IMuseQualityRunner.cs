using MuseQCApp.Models;
using MuseQCDBAccess.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for methods to run the Muse quality scripts
/// </summary>
public interface IMuseQualityRunner
{
    /// <summary>
    /// Run the muse quality check software on the given edf file
    /// </summary>
    /// <param name="edfPath">The path to the edf file</param>
    /// <param name="outputPath">The path to the directory where the output data should be stored</param>
    /// <returns>The full paths of output files</returns>
    public MuseQualityOutputPaths? RunMuseQualityCheck(string edfPath, string outputPath);

    /// <summary>
    /// Read data from the output csv
    /// </summary>
    /// <param name="csvPath"></param>
    /// <returns></returns>
    public QCStatsModel? ReadOutputCsv(string csvPath);
}
