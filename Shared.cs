using System.Collections.Immutable;
using System.IO;

namespace Brendan_Stock_Media_Distributor;
public static partial class Shared
{
    public static SettingsObject CurrentSettings { get; set; }
    public static ImmutableHashSet<char> PathDisallowedChars { get; }
    static Shared()
    {
        CurrentSettings = null!;
        PathDisallowedChars = [.. Path.GetInvalidFileNameChars()];
    }
    public static string? GetMimeType(string Extension)
    {
        return Extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".mov" => "video/mov",
            ".mpeg" => "video/mpeg",
            ".mp4" => "video/mp4",
            ".mpg" => "video/mpg",
            ".avi" => "video/avi",
            ".wmv" => "video/wmv",
            ".mpegps" => "video/mpegps",
            ".flv" => "video/flv",
            _ => null,
        };
    }
}