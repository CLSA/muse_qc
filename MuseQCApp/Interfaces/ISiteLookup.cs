using MuseQCApp.Constants;

namespace MuseQCApp.Interfaces;

/// <summary>
/// An interface with methods to determine the site a participant is ascociated with
/// </summary>
public interface ISiteLookup
{
    /// <summary>
    /// Lookup a participants site using their weston ID
    /// </summary>
    /// <param name="westonId">The weston ID of the participant to check</param>
    /// <returns>The site that participant belongs too</returns>
    public SiteLocation LookupUsingWestonID(string westonId);
}
