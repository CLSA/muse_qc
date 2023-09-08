using MuseQCDBAccess.Models;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for implementing rules to make muse quality decisions
/// </summary>
public interface IMuseQualityDecisions
{
    /// <summary>
    /// Determines if the day is an actual day of data.
    /// NOTE: Sometimes tests are done for small amounts of time to ensure 
    /// everything is working with the Muse headband. These tests should not 
    /// be counted as an actual night of data
    /// </summary>
    /// <param name="stats">The QC stats to evaluate</param>
    /// <returns>True if this is an actual night of data, false otherwise</returns>
    public bool IsActualNight(QCStatsModel stats);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stats">The QC stats to evaluate</param>
    /// <returns>True if there is a duration problem, false otherwise</returns>
    public bool HasDurationProblem(QCStatsModel stats);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stats">The QC stats to evaluate</param>
    /// <returns>True if there is a problem with quality other than duration, false otherwise</returns>
    public bool HasQualityProblem(QCStatsModel stats);

    /// <summary>
    /// Get the version number of the implementation
    /// </summary>
    /// <returns>The version number for the implementation</returns>
    public int GetVersionNumber();
}
