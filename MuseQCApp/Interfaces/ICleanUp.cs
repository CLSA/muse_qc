namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for methods to clean up the file system
/// </summary>
public interface ICleanUp
{
    /// <summary>
    /// Removes any edf files that have output files created already and remove formatted 
    /// edf files produced by quality script
    /// </summary>
    public void RemoveUnnecessaryFiles();
}
