namespace MuseQCDBAccess.Models;

/// <summary>
/// A model that contains relevant quality informtion on 
/// an eeg file for building reports
/// </summary>
public class EegQualityReportModel
{
    /// <summary>
    /// The participants weston id
    /// </summary>
    public string WestonID { get; init; }

    /// <summary>
    /// The site the participant collected data at
    /// </summary>
    public string Site { get; init; }

    /// <summary>
    /// The date time when the collection started
    /// </summary>
    public DateTime StartDateTime { get; init; }

    /// <summary>
    /// The date time of when the file was uploaded to the Muse google bucket
    /// </summary>
    public DateTime UploadDateTime { get; init; }

    /// <summary>
    /// The path to the jpg created by the quality script
    /// </summary>
    public string JpgPath { get; init; }

    /// <summary>
    /// The duration of the collection
    /// </summary>
    public double Duration { get; init; }

    public bool LessThan5HoursDuration => Duration < 5;

    /// <summary>
    /// The FT any quality value
    /// </summary>
    public double FtAny { get; init; }

    /// <summary>
    /// True if their was a problem with frontal or temporal contacts
    /// </summary>
    public bool QualityProblem => FtAny < 0.8;

    /// <summary>
    /// The F any quality value
    /// </summary>
    public double FAny { get; init; }

    /// <summary>
    /// True if their was a problem with the frontal contacts
    /// </summary>
    public bool FrontalProblem => FAny < 0.8;

    /// <summary>
    /// The T any quality value
    /// </summary>
    public double TAny { get; init; }

    /// <summary>
    /// True if their was a problem with the temporal contacts
    /// </summary>
    public bool TemporalProblem => TAny < 0.8;

    /// <summary>
    /// True if this file has any problems
    /// </summary>
    public bool HasProblem => LessThan5HoursDuration || QualityProblem;

    /// <summary>
    /// A string of what the problem is
    /// </summary>
    public string ProblemsStr => $"{(LessThan5HoursDuration ? $"Dur,": "")}" +
        $"{(FrontalProblem ? $"Front," : "")}" +
        $"{(TemporalProblem ? $"Temp," : "")}";

}
