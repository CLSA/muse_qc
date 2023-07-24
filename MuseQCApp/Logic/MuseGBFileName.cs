namespace MuseQCApp.Logic;

/// <summary>
/// Functions to infer data based on the Muse Google bucket file name
/// Format: 
/// [DateTime]-[Timezone offset]_[Pod ID]_[Weston ID]_[Data type]
/// [yyyy-MM-ddThh:mm:ss]-[hh:mm]_[XXXX-XXXX-XXXX]_[ww########]_XXX
/// 
/// Sample file name:
///     2023-06-18T00:31:31-04:00_6002-CNZB-5F0A_ww75958498_acc
///     [DateTime]        [Offset]   [Pod ID]    [Weston ID] [Data type]
/// </summary>
public static class MuseGBFileName
{
    /// <summary>
    /// Get the start date time from the google bucket file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The start datetime if it could be parsed, otherwise null</returns>
    public static DateTime? GetStartDateTime(string fileName)
    {
        try
        {
            string startDateTimeStr = fileName.Substring(0, 19);
            bool dateParsed = DateTime.TryParse(startDateTimeStr, out DateTime startDateTime);
            if (dateParsed)
            {
                return startDateTime;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the time zone offset from the google bucket  file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The time zone offset if it could be parsed, otherwise null</returns>
    public static float? GetTimezoneOffset(string fileName)
    {
        try
        {
            string hourStr = fileName.Substring(20, 2);
            string minStr = fileName.Substring(23, 2);
            bool hourParsed = int.TryParse(hourStr, out int hour);
            bool minParsed = int.TryParse(minStr, out int minute);
            if (hourParsed & minParsed)
            {
                float offset = (float)(hour + (minute / 60.0));
                return fileName[19].Equals('-') ? offset * -1 : offset;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the pod id from the google bucket file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The pod id if it could be parsed, otherwise null</returns>
    public static string? GetPodID(string fileName)
    {
        try
        {
            string podIdStr = fileName.Substring(26, 14);
            if (podIdStr[4] == '-' & podIdStr[9] == '-')
            {
                return podIdStr;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the weston id from the google bucket file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The weston id if it could be parsed, otherwise null</returns>
    public static string? GetWestonID(string fileName)
    {
        try
        {
            string westonIdStr = fileName.Substring(41, 10);
            if (westonIdStr.ToLower().StartsWith("ww") || westonIdStr.ToLower().StartsWith("tt"))
            {
                return westonIdStr;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Get the data type from the google bucket file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The data type if it could be parsed, otherwise null</returns>
    public static string? GetDataType(string fileName)
    {
        try
        {
            return fileName.Split("_").Last();
        }
        catch { }
        return null;
    }
}
