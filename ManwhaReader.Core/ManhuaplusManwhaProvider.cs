using HtmlAgilityPack;
using ManwhaReader.Core.DatabaseObjects;
using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.Core;

public class ManhuaplusManwhaProvider : IManwhaProvider
{
    public string Name => "ManhuaPlus";
    public string ImageUrl => ImagePathProvider.ProviderManhuaPlus;
    
    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchQuery));

        using var client = HttpClientProvider.CreateHttpClient();
        client.DefaultRequestHeaders.Add("Referer", "https://manhuaplus.com/");
        
        var url = $"https://manhuaplus.com/?s={searchQuery}&post_type=wp-manga";
        
        var htmlString = await client.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);
        
        var nodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tab-thumb') and contains(@class, 'c-image-hover')]")
            .Select(x => x.ChildNodes.First(c => c.Name == "a")).ToList();

        if (nodes.Count == 0)
            return [];
        
        var titles = nodes.Select(x => x.GetAttributeValue("title", string.Empty)).ToList();
        var thumbnailUrls = nodes.Select(x => x.ChildNodes.First(c => c.Name == "img").GetAttributeValue("data-src", ""))
            .ToList();
        
        var list = new List<IManwhaSearchResult>();
        
        for (var i = 0; i < titles.Count; i++)
        {
            byte[] imageData = [];
            
            if (loadImages)
            {
                try
                {
                    imageData = await client.GetByteArrayAsync(thumbnailUrls[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Thumbnail Url: {thumbnailUrls[i]}");
                    Console.WriteLine(e);
                }
            }
            
            list.Add(new ManwhaSearchResult
            {
                Title = titles[i],
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