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
    /// True if the collection was flagged for having a short duration, false otherwise
    /// </summary>
    public bool HasDurationProblem { get; init; }

    /// <summary>
    /// True if the collection was flagged for have an issue with the signal quality
    /// </summary>
    public bool HasQualityProblem { get; init; }

    /// <summary>
    /// The version of the algorithm used for determining problems
    /// </summary>
    public int MuseQualityVersion { get; init; }

    /// <summary>
    /// The duration of the collection
    /// </summary>
    public double Duration { get; init; }

    public bool LessThan5HoursDuration => Duration < 5;

    /// <summary>
    /// The FT any quality value
    /// </summary>
    public double FtAny { get; init; }

}
