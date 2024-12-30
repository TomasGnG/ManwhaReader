using DevExpress.Xpo;
using ManwhaReader.Core.DatabaseObjects;
using ManwhaReader.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace ManwhaReader.Core;

public class MangadexManwhaProvider : IManwhaProvider
{
    public string Name => "MangaDex";
    public string ImageUrl => ImagePathProvider.ProviderMangaDex;
    
    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchQuery));

        var url = $"https://api.mangadex.org/manga?limit=10&title={searchQuery}";
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
                ImageData = loadImages ? GetManwhaCoverImageData(jsonObject["id"]?.ToString()!, coverId?.ToString()!) : []
            });
        }

        return list;
    }

    public async Task<Manwha> GetManwhaByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(title));
        
        using var client = CreateHttpClient();
        var url = $"https://api.mangadex.org/manga?title={title}";
        var jsonString = await client.GetStringAsync(url);
        
        if (JObject.Parse(jsonString)["data"] is not JArray jsonArray)
            throw new InvalidOperationException("Response data cannot be null.");
        
        var jObject = jsonArray.FirstOrDefault(x => string.Equals(title, x["attributes"]?["title"]?.First?.First?.ToString(), StringComparison.InvariantCultureIgnoreCase));
        
        if(jObject == null)
            throw new Exception($"Couldn't find manga for title '{title}'\nPlease report this.");
        
        var description = jObject["attributes"]?["description"]?.First?.First?.ToString();
        var status = jObject["attributes"]?["status"]?.ToString();
        var tags = new List<string>();

        if (jObject["attributes"]?["tags"] is JArray tagsArray)
        {
            tags.AddRange(tagsArray.Select(tagObject => tagObject["attributes"]?["name"]?.First?.First?.ToString()!)
                .Where(tag => !string.IsNullOrWhiteSpace(tag)));
        }
        
        var manwhaId = jObject["id"]?.ToString();
        var coverId = jObject["relationships"]?.FirstOrDefault(x => string.Equals("cover_art", x["type"]?.ToString(), StringComparison.InvariantCultureIgnoreCase))?["id"]?.ToString();
        var thumbnailUrl = GetManwhaCoverImageUrl(manwhaId, coverId);

        return new Manwha(Session.DefaultSession)
        {
            Name = title,
            ThumbnailUrl = thumbnailUrl,
            Description = description,
            Status = status,
            Tags = tags
        };
    }

    private static byte[] GetManwhaCoverImageData(string manwhaId, string coverId)
    {
        using var client = CreateHttpClient();
        return client.GetByteArrayAsync(GetManwhaCoverImageUrl(manwhaId, coverId)).Result;
    }

    private static string GetManwhaCoverImageUrl(string? manwhaId, string? coverId)
    {
        if (string.IsNullOrWhiteSpace(manwhaId) || string.IsNullOrWhiteSpace(coverId))
            return ImagePathProvider.ThumbnailNotFound;
        
        using var client = CreateHttpClient();
        
        var coverUrlRequest = $"https://api.mangadex.org/cover/{coverId}";
        var coverUrlResponse = client.GetStringAsync(coverUrlRequest).Result;
        var fileName = JObject.Parse(coverUrlResponse)["data"]?["attributes"]?["fileName"]?.ToString();
        
        return $"https://uploads.mangadex.org/covers/{manwhaId}/{fileName}";
    }
    
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0");
        return client;
    }
}