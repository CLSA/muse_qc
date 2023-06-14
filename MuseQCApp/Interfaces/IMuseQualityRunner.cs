namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface for methods to run the Muse quality scripts
/// </summary>
public interface IMuseQualityRunner
{
    public void RunMuseQualityCheck(string edfPath, string outputPath);
}
