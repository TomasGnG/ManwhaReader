namespace ManwhaReader.Core;

public class ManwhaProviderImageProvider
{
    private const string BaseUrl = "/Images/Providers/";
    
    public static string Thunderscans => $"{BaseUrl}thunderscans.png";
    public static string MangaDex => $"{BaseUrl}mangadex.png";
}