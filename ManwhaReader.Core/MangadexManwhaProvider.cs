using ManwhaReader.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace ManwhaReader.Core;

public class MangadexManwhaProvider : IManwhaProvider
{
    public string Name => "MangaDex";
    public string ImageUrl => ManwhaProviderImageProvider.MangaDex;
    
    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchQuery));

        var url = $"https://api.mangadex.org/manga?limit=20&title={searchQuery}";
        using var client = CreateHttpClient();
        var jsonString = await client.GetStringAsync(url);
        
        var jsonArray = JObject.Parse(jsonString)["data"] as JArray;

        if (jsonArray == null)
            return [];
        
        var list = new List<IManwhaSearchResult>();
        
        foreach (var jsonObject in jsonArray.Cast<JObject>())
        {
            var relations = jsonObject["relationships"] as JArray;
            var coverId = relations?.Cast<JObject>()
                .FirstOrDefault(x => string.Equals("cover_art", x["type"]?.ToString(), StringComparison.InvariantCultureIgnoreCase))?["id"];
            
            list.Add(new ManwhaSearchResult
            {
                Title = jsonObject["attributes"]?["title"]?.First?.First?.ToString()!,
                ImageData = GetManwhaCoverImageData(jsonObject["id"]?.ToString()!, coverId?.ToString()!)
            });
        }

        return list;
    }

    private static byte[] GetManwhaCoverImageData(string manwhaId, string coverId)
    {
        using var client = CreateHttpClient();
        var coverUrlRequest = $"https://api.mangadex.org/cover/{coverId}";
        var coverUrlResponse = client.GetStringAsync(coverUrlRequest).Result;
        
        var coverFileName = JObject.Parse(coverUrlResponse)["data"]?["attributes"]?["fileName"]?.ToString();
        
        return client.GetByteArrayAsync($"https://uploads.mangadex.org/covers/{manwhaId}/{coverFileName}").Result;
    }
    
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ManwhaReader");
        return client;
    }
}