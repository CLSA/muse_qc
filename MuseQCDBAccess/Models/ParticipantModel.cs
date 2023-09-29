namespace MuseQCDBAccess.Models;
public class ParticipantModel
{
    /// <summary>
    /// The westonID assigned to the participant
    /// </summary>
    public string WestonID { get; set; }

    /// <summary>
    /// The site that the participant went to for data collection
    /// </summary>
    public string? Site { get; set; }
}
