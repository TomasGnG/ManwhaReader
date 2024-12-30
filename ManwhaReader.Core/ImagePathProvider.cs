namespace ManwhaReader.Core;

public static class ImagePathProvider
{
    private const string ImagesFolder = "/Images/";
    
    private const string ProviderBaseUrl = $"{ImagesFolder}Providers/";
    public static string ProviderMangaDex => $"{ProviderBaseUrl}mangadex.png";
    public static string ProviderReaperscans => $"{ProviderBaseUrl}reaperscans.png";
    public static string ProviderManhuaPlus => $"{ProviderBaseUrl}manhuaplus.png";
    
    public static string ThumbnailNotFound => $"{ImagesFolder}notfound.png";
}