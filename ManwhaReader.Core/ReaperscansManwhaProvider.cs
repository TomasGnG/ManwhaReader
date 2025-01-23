using ManwhaReader.Core.DatabaseObjects;
using ManwhaReader.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace ManwhaReader.Core;

public class ReaperscansManwhaProvider : IManwhaProvider
{
    public string Name => "Reaperscans";
    public string ImageUrl => ImagePathProvider.ProviderReaperscans;
    
    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchQuery));

        using var client = HttpClientProvider.CreateHttpClient();
        var url = $"https://api.reaperscans.com/query?adult=true&query_string={searchQuery}";
        var jsonString = await client.GetStringAsync(url);

        if (string.IsNullOrWhiteSpace(jsonString))
            return [];

        if (JObject.Parse(jsonString)["data"] is not JArray jObjects)
            return [];
        
        var list = new List<IManwhaSearchResult>();
        foreach (var jObject in jObjects)
        {
            if(list.Count == 10)
                break;
            
            var title = jObject["title"]?.ToString();

            byte[] imageData = [];
            
            if (loadImages)
            {
                var rawThumbnailUrl = jObject["thumbnail"]?.ToString();
                var thumbnailUrl = !string.IsNullOrWhiteSpace(rawThumbnailUrl) && rawThumbnailUrl.Contains("https://media.reaperscans.com/file/4SRBHm/")
                    ? rawThumbnailUrl
                    : $"https://media.reaperscans.com/file/4SRBHm/{rawThumbnailUrl}";

                imageData = await client.GetByteArrayAsync(thumbnailUrl);
            }
            
            list.Add(new ManwhaSearchResult
            {
                Title = title!,
                ImageData = imageData
            });
        }
        
        return list;
    }

    public Task<Manwha> GetManwhaByTitle(string title)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Chapter>> GetChaptersByManwhaTitle(string title)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ChapterImageUrl>> GetChapterImageUrls(string manwhaTitle, double chapterNumber)
    {
        throw new NotImplementedException();
    }
}