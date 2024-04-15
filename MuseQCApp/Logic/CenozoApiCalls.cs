using MuseQCApp.Models;
using System.Text.Json;

namespace MuseQCApp.Logic;
public class CenozoApiCalls
{
    /// <summary>
    /// Updates the Weston site lookup csv with information pulled from Cenozo api call
    /// </summary>
    /// <param name="credentials">The credentials to make a call to Cenozo</param>
    /// <param name="siteLookupURL">The url to needed to make the api call</param>
    /// <param name="siteLookupPath">The full path of the Weston site lookup csv</param>
    public static void UpdateSiteLookupCsv(string credentials, string siteLookupURL, string siteLookupPath)
    {
        // don't try to call api if any value is null or empty
        if(string.IsNullOrEmpty(credentials) || string.IsNullOrEmpty(siteLookupURL) || string.IsNullOrEmpty(siteLookupPath)) { return; }
        
        
        using HttpClient client = new();
        
        client.DefaultRequestHeaders.Add("Authorization", $"Basic {Base64Encode(credentials)}");

        string json = client.GetStringAsync(siteLookupURL).Result;
        List<CenozoParticipantSiteInfoModel> participantSites = JsonSerializer.Deserialize<List<CenozoParticipantSiteInfoModel>>(json);

        // Return if participant site snull or empty
        if(participantSites == null || participantSites.Count == 0 ) return;

        // Write new lookup table based on updated information
        using(StreamWriter sw = new(siteLookupPath))
        {
            sw.WriteLine("Weston ID,Site");
            foreach(var participant in participantSites)
            {
                sw.WriteLine($"{participant.wwid},{participant.site}");
            }
        }
    }

    /// <summary>
    /// Encode plain text as base 64
    /// </summary>
    /// <param name="plainText">The text to encode</param>
    /// <returns>The base 64 encoded string</returns>
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
