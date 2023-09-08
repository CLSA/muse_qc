namespace MuseQCApp.Models;

/// <summary>
/// A model to store full paths for the output files created by the muse quality script
/// </summary>
public class MuseQualityOutputPathsModel
{
    /// <summary>
    /// The full path to the output jpg file
    /// </summary>
    public string JpgPath { get; init; }

    /// <summary>
    /// The full path to the output csv file
    /// </summary>
    public string CsvPath { get; init; }

    /// <summary>
    /// The full path to the output edf file
    /// </summary>
    public string EdfPath { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="jpgPath">The full path to the output jpg file</param>
    /// <param name="csvPath">The full path to the output csv file</param>
    /// <param name="edfPath">The full path to the output edf file</param>
    public MuseQualityOutputPathsModel(string jpgPath, string csvPath, string edfPath)
    {
        JpgPath = jpgPath;
        CsvPath = csvPath;
        EdfPath = edfPath;
    }
}
