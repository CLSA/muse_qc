using Microsoft.Extensions.Logging;
using MuseQCDBAccess.Models;

namespace MuseQCApp.Logic;

/// <summary>
/// Functions to interact with the Site lookup table provided by Patrick Edmond
/// The site lookup table is a csv with 2 columns (Weston ID,Site)
/// The weston IDs have 2 capital WW and then 8 numbers
/// The sites are spelled out in long format with the fully name of the site and then DCS
/// As of Sep 2023, the sites are the sites that the participant collected data at for FU3
/// NOTE: A new site line may need to be added to the DB for each follow after FU3 if site is expected to change
/// </summary>
public static class SiteLookupTable
{

    #region Site strings

    /// <summary>
    /// The 3 char short form for the Calgary DCS
    /// </summary>
    private const string CalgaryShort = "Cal";

    /// <summary>
    /// The long form from the site lookup table for the calgary DCS
    /// </summary>
    private const string CalgaryLong = "Calgary DCS";

    /// <summary>
    /// The 3 char short form for the Dalhousie DCS
    /// </summary>
    private const string DalhousieShort = "Dal";

    /// <summary>
    /// The long form from the site lookup table for the Dalhousie DCS
    /// </summary>
    private const string DalhousieLong = "Dalhousie DCS";

    /// <summary>
    /// The 3 char short form for the Hamilton DCS
    /// </summary>
    private const string HamiltonShort = "Ham";

    /// <summary>
    /// The long form from the site lookup table for the Hamilton DCS
    /// </summary>
    private const string HamiltonLong = "Hamilton DCS";

    /// <summary>
    /// The 3 char short form for the Manitoba DCS
    /// </summary>
    private const string ManitobaShort = "Man";

    /// <summary>
    /// The long form from the site lookup table for the Manitoba DCS
    /// </summary>
    private const string ManitobaLong = "Manitoba DCS";

    /// <summary>
    /// The 3 char short form for the McGill DCS
    /// </summary>
    private const string McGillShort = "McG";

    /// <summary>
    /// The long form from the site lookup table for the McGill DCS
    /// </summary>
    private const string McGillLong = "McGill DCS";

    /// <summary>
    /// The 3 char short form for the Memorial DCS
    /// </summary>
    private const string MemorialShort = "Mem";

    /// <summary>
    /// The long form from the site lookup table for the Memorial DCS
    /// </summary>
    private const string MemorialLong = "Memorial DCS";

    /// <summary>
    /// The 3 char short form for the Ottawa DCS
    /// </summary>
    private const string OttawaShort = "Ott";

    /// <summary>
    /// The long form from the site lookup table for the Ottawa DCS
    /// </summary>
    private const string OttawaLong = "Ottawa DCS";

    /// <summary>
    /// The 3 char short form for the Sherbrooke DCS
    /// </summary>
    private const string SherbrookeShort = "She";

    /// <summary>
    /// The long form from the site lookup table for the Sherbrooke DCS
    /// </summary>
    private const string SherbrookeLong = "Sherbrooke";

    /// <summary>
    /// The 3 char short form for the Simon Fraser DCS
    /// </summary>
    private const string SimonFraserShort = "SFU";

    /// <summary>
    /// The long form from the site lookup table for the Simon Fraser DCS
    /// </summary>
    private const string SimonFraserLong = "Simon Fraser DCS";

    /// <summary>
    /// The 3 char short form for the University of BC DCS
    /// </summary>
    private const string UniversityofBCShort = "UBC";

    /// <summary>
    /// The long form from the site lookup table for the University of BC DCS
    /// </summary>
    private const string UniversityofBCLong = "University of BC DCS";

    /// <summary>
    /// The 3 char short form for the Victoria DCS
    /// </summary>
    private const string VictoriaShort = "Vic";

    /// <summary>
    /// The long form from the site lookup table for the Victoria DCS
    /// </summary>
    private const string VictoriaLong = "Victoria DCS";

    #endregion

    /// <summary>
    /// Reads the site lookup table csv and converts each entry into a <see cref="ParticipantModel"/>
    /// </summary>
    /// <param name="csvPath">The site lookup table csv that has the info to read in</param>
    /// <param name="logger">A logger to log any errors to if provided</param>
    /// <returns>A list of <see cref="ParticipantModel"/></returns>
    public static List<ParticipantModel> ReadSiteLookupTableCsv(string csvPath, ILogger? logger = null)
    {
        // Create output list
        List<ParticipantModel> participants = new();
        try
        {
            // log error if the csv does not exist
            if (File.Exists(csvPath) == false)
                throw new Exception($"Csv does not exist: {csvPath}");

            // setup stream reader
            using StreamReader sr = new StreamReader(csvPath);

            // log error if the csv file is empty
            if (sr.EndOfStream)
                throw new Exception($"No data in csv: {csvPath}");

            // read header
            string? header = sr.ReadLine();

            // log error if the header is not as expected
            const string expectedHeader = "Weston ID,Site";
            if (expectedHeader.Equals(header) == false)
                throw new Exception($"Header does not match the expected value. Expected header: {header} Actual Header: {header}");

            // Convert each line of file into participant models
            // and add to participants list
            while (sr.EndOfStream == false)
            {
                string? line = sr.ReadLine();
                ParticipantModel? participant = GetParticipantFromLookupString(line, logger);

                if (participant == null) continue;

                participants.Add(participant);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError($"{ex.Message}");
        }

        return participants;
    }

    /// <summary>
    /// Convert a string line from the lookup table csv into a <see cref="ParticipantModel"/>
    /// </summary>
    /// <param name="lookupLine">The line to convert</param>
    /// <param name="logger">A logger to log any errors to if provided</param>
    /// <returns>a <see cref="ParticipantModel"/> if it could be converted or null otherwise</returns>
    private static ParticipantModel? GetParticipantFromLookupString(string? lookupLine, ILogger? logger = null)
    {
        // Check lookup line to ensure not null, empty or whitespace
        if (string.IsNullOrWhiteSpace(lookupLine))
        {
            return null;
        }

        // Split line on commas
        string[] lineSplit = lookupLine.Split(",");

        // Ensure atleast 2 entries in the line split
        if (lineSplit.Length < 2)
        {
            logger?.LogError($"Unexpected line in lookup table csv. Line: {lookupLine}");
            return null;
        }

        // Check if weston id formatted as expected
        string westonID = lineSplit[0];
        if (IsWestonID(lineSplit[0]) == false)
        {
            logger?.LogError($"{lineSplit[0]}");
            return null;
        }

        // Check if site formatted as expected
        string siteLong = lineSplit[1];
        string siteShort = GetSiteShortForm(siteLong);
        if (string.IsNullOrWhiteSpace(siteShort))
        {
            logger?.LogError($"Could not identify the DCS site in lookup table csv from the string {siteLong}");
            return null;
        }

        // Return participant model
        return new ParticipantModel()
        {
            WestonID = westonID,
            Site = siteShort
        };
    }

    /// <summary>
    /// Checks if a string is a weston ID
    /// </summary>
    /// <param name="possibleWestonID">The id to check</param>
    /// <returns>True if the string is a correctly formatted weston ID, false otherwise</returns>
    private static bool IsWestonID(string possibleWestonID)
    {
        // Remove white space and make id lower case for comparison
        string cleanPossibleWestonID = possibleWestonID.Trim().ToLower();

        // Return false unless the id starts with ww and is 10 characters long
        bool startsWW = cleanPossibleWestonID.ToLower().StartsWith("ww");
        bool length10 = cleanPossibleWestonID.Trim().Length == 10;
        if(startsWW == false || length10 == false) return false;

        // Return true if the last 8 digits of the weston id are all numbers
        bool parsed = int.TryParse(cleanPossibleWestonID.Substring(2), out int intVal);
        return parsed;
    }


    /// <summary>
    /// Coverts a long form site string into a short form site string
    /// </summary>
    /// <param name="siteLongForm">The long form version of the site string</param>
    /// <returns>The short form string of the site or an empty string if the long form could not be identified</returns>
    private static string GetSiteShortForm(string siteLongForm)
    {
        switch(siteLongForm)
        {
            case CalgaryLong: return CalgaryShort;
            case DalhousieLong: return DalhousieShort;
            case HamiltonLong: return HamiltonShort;
            case ManitobaLong: return ManitobaShort;
            case McGillLong: return McGillShort;
            case MemorialLong: return MemorialShort;
            case OttawaLong: return OttawaShort;
            case SherbrookeLong: return SherbrookeShort;
            case SimonFraserLong: return SimonFraserShort;
            case UniversityofBCLong: return UniversityofBCShort;
            case VictoriaLong: return VictoriaShort;
            default: return string.Empty;
        }
    }

}
