using MuseQCApp.Interfaces;

namespace MuseQCApp.Modules;

public class GoogleBucket : IGoogleBucket
{
    public bool DownloadFiles(List<string> filePaths, string storageDirPath)
    {
        throw new NotImplementedException();
    }

    public List<string> GetFilePaths()
    {
        throw new NotImplementedException();
    }
}
