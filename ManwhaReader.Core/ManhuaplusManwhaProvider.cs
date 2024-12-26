using HtmlAgilityPack;
using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.Core;

public class ManhuaplusManwhaProvider : IManwhaProvider
{
    public string Name => "ManhuaPlus";
    public string ImageUrl => ManwhaProviderImagePaths.ManhuaPlus;
    
    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchQuery));

        using var client = CreateHttpClient();
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
    
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", "https://manhuaplus.com/");
        return client;
    }   
}