namespace MuseQCDBAccess.Models;

/// <summary>
/// A model storing the information from the primary key of the Collection data table
/// </summary>
public class CollectionDataPrimaryKeyModel
{
    public string? westonID { get; set; }
    public string? podID { get; set; }
    public DateTime? startDateTime { get; set; }
}
