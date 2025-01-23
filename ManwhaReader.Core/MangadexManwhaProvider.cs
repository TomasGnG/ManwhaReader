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
        using var client = HttpClientProvider.CreateHttpClient();
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
                ImageData = loadImages ? await GetManwhaCoverImageData(jsonObject["id"]?.ToString()!, coverId?.ToString()!) : []
            });
        }

        return list;
    }

    public async Task<Manwha> GetManwhaByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(title));
        
        var manwhaObject = await GetManwhaObjectByTitle(title);
        
        var description = manwhaObject["attributes"]?["description"]?.First?.First?.ToString();
        var status = manwhaObject["attributes"]?["status"]?.ToString();
        var tags = new List<string>();

        if (manwhaObject["attributes"]?["tags"] is JArray tagsArray)
        {
            tags.AddRange(tagsArray.Select(tagObject => tagObject["attributes"]?["name"]?.First?.First?.ToString()!)
                .Where(tag => !string.IsNullOrWhiteSpace(tag)));
        }
        
        var manwhaId = manwhaObject["id"]?.ToString();
        var coverId = manwhaObject["relationships"]?.FirstOrDefault(x => string.Equals("cover_art", x["type"]?.ToString(), StringComparison.InvariantCultureIgnoreCase))?["id"]?.ToString();
        var thumbnailUrl = await GetManwhaCoverImageUrl(manwhaId, coverId);
        
        var manwha = Session.DefaultSession.Query<Manwha>().FirstOrDefault(x => x.Name == title) ?? new Manwha(Session.DefaultSession)
        {
            Name = title,
            ThumbnailUrl = thumbnailUrl,
            Description = description,
            Status = status,
            Tags = string.Join(';', tags)
        };
        
        manwha.Save();
        return manwha; 
    }

    public async Task<IEnumerable<Chapter>> GetChaptersByManwhaTitle(string title)
    {
        var manwhaObject = await GetManwhaObjectByTitle(title);
        var manwhaId = manwhaObject["id"]?.ToString();
        var chapterNumbers = new HashSet<double>();
        
        using var client = HttpClientProvider.CreateHttpClient();

        var offset = 0;
        var alreadyQueried = 0;
        
        var url = $"https://api.mangadex.org/manga/{manwhaId}/feed?limit=500&offset={offset}&translatedLanguage[]=en&order[chapter]=asc&includeExternalUrl=0&includeFuturePublishAt=0";
        var jsonObject = JObject.Parse(await client.GetStringAsync(url));
        
        do
        {
            var jArray = JArray.Parse(jsonObject["data"]?.ToString());
            
            foreach (var jToken in jArray)
            {
                chapterNumbers.Add(double.Parse(jToken["attributes"]?["chapter"]?.ToString()));
            }
            
            offset += 500;
            url = $"https://api.mangadex.org/manga/{manwhaId}/feed?limit=500&offset={offset}&translatedLanguage[]=en&order[chapter]=asc&includeExternalUrl=0&includeFuturePublishAt=0";
            jsonObject = JObject.Parse(await client.GetStringAsync(url));
            alreadyQueried += int.Parse(jsonObject["total"]?.ToString()) - alreadyQueried > 500 ? 500 : int.Parse(jsonObject["total"]?.ToString())-alreadyQueried;
        }
        while (alreadyQueried != int.Parse(jsonObject["total"]?.ToString()));
        
        var chapters = new List<Chapter>();
        
        using var uow = new UnitOfWork();
        foreach (var chapterNumber in chapterNumbers)
        {
            var savedManwha = Session.DefaultSession.Query<Manwha>().First(x => x.Name.Contains(title));
            var chapter = Session.DefaultSession.Query<Chapter>().FirstOrDefault(x => x.Manwha == savedManwha.ManwhaId && x.Number == chapterNumber) ?? new Chapter(uow);
            chapter.Manwha = savedManwha.ManwhaId;
            chapter.Number = chapterNumber;

            chapters.Add(chapter);
        }
        
        await uow.CommitChangesAsync();
        return chapters;
    }

    public async Task<IEnumerable<ChapterImageUrl>> GetChapterImageUrls(string manwhaTitle, double chapterNumber)
    {
        var manwhaObject = await GetManwhaObjectByTitle(manwhaTitle);
        var manwhaId = manwhaObject["id"]?.ToString();
        
        using var client = HttpClientProvider.CreateHttpClient();
        var offset = 0;
        var url = $"https://api.mangadex.org/manga/{manwhaId}/feed?limit=500&offset={offset}&translatedLanguage[]=en&order[chapter]=asc&includeExternalUrl=0&includeFuturePublishAt=0";
        var jsonObject = JObject.Parse(await client.GetStringAsync(url));
        
        var chapterId = ((JArray) jsonObject["data"]!).FirstOrDefault(chapter => double.Parse(chapter["attributes"]?["chapter"]?.ToString()!).Equals(chapterNumber));
        
        while (chapterId == null)
        {
            if(offset > int.Parse(jsonObject["total"]?.ToString()))
                break;
            
            offset += 500;
            url = $"https://api.mangadex.org/manga/{manwhaId}/feed?limit=500&offset={offset}&translatedLanguage[]=en&order[chapter]=asc&includeExternalUrl=0&includeFuturePublishAt=0";
            jsonObject = JObject.Parse(await client.GetStringAsync(url));
            chapterId = ((JArray) jsonObject["data"]!).FirstOrDefault(chapter => double.Parse(chapter["attributes"]?["chapter"]?.ToString()!).Equals(chapterNumber));
        }

        if (chapterId == null)
            throw new InvalidOperationException("Chapter not found");
        
        chapterId = chapterId["id"].ToString();
        
        var atHomeUrl = $"https://api.mangadex.org/at-home/server/{chapterId}";
        var atHomeObject = JObject.Parse(await client.GetStringAsync(atHomeUrl));
        
        var baseUrl = atHomeObject["baseUrl"]?.ToString();
        var hash = atHomeObject["chapter"]?["hash"]?.ToString();
        
        if (atHomeObject["chapter"]?["data"] is not JArray imageUrlsArray)
            throw new InvalidOperationException($"No image urls found for chapter {chapterNumber} with the id {chapterId}.");
        
        var results = new List<ChapterImageUrl>();
        
        for (var i = 0; i < imageUrlsArray.Count; i++)
        {
            var manwha = Session.DefaultSession.Query<Manwha>().First(x => x.Name.Contains(manwhaTitle));
            var chapter = Session.DefaultSession.Query<Chapter>().FirstOrDefault(x => x.Number == chapterNumber && x.Manwha == manwha.ManwhaId);
            var chapterImageUrl = Session.DefaultSession.Query<ChapterImageUrl>().FirstOrDefault(x => x.Chapter == chapter!.ChapterId && x.UrlNumber == i) 
                                  ?? new ChapterImageUrl(Session.DefaultSession);
            
            chapterImageUrl.Chapter = Session.DefaultSession.Query<Chapter>().First(x => x.Number == chapterNumber).ChapterId;
            chapterImageUrl.UrlNumber = i;
            chapterImageUrl.Url = $"{baseUrl}/data/{hash}/{imageUrlsArray[i]}";
            
            
            // chapterImageUrl.Save();
            results.Add(chapterImageUrl);
        }
        
        return results;
    }

    private static async Task<JToken> GetManwhaObjectByTitle(string title)
    {
        using var client = HttpClientProvider.CreateHttpClient();
        var url = $"https://api.mangadex.org/manga?title={title}";
        var jsonString = await client.GetStringAsync(url);
        
        if (JObject.Parse(jsonString)["data"] is not JArray jsonArray)
            throw new InvalidOperationException("Response data cannot be null.");
        
        var jObject = jsonArray.FirstOrDefault(x => string.Equals(title, x["attributes"]?["title"]?.First?.First?.ToString(), StringComparison.InvariantCultureIgnoreCase));
        
        if(jObject == null)
            throw new Exception($"Couldn't find manga for title '{title}'\nPlease report this.");
        
        return jObject;
    }

    private static async Task<byte[]> GetManwhaCoverImageData(string manwhaId, string coverId)
    {
        using var client = HttpClientProvider.CreateHttpClient();
        return await client.GetByteArrayAsync(await GetManwhaCoverImageUrl(manwhaId, coverId));
    }

    private static async Task<string> GetManwhaCoverImageUrl(string? manwhaId, string? coverId)
    {
        if (string.IsNullOrWhiteSpace(manwhaId) || string.IsNullOrWhiteSpace(coverId))
            return ImagePathProvider.ThumbnailNotFound;
        
        using var client = HttpClientProvider.CreateHttpClient();
        
        var coverUrlRequest = $"https://api.mangadex.org/cover/{coverId}";
        var coverUrlResponse = await client.GetStringAsync(coverUrlRequest);
        var fileName = JObject.Parse(coverUrlResponse)["data"]?["attributes"]?["fileName"]?.ToString();
        
        return $"https://uploads.mangadex.org/covers/{manwhaId}/{fileName}";
    }
}