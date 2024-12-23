using HtmlAgilityPack;

namespace ManwhaReader.Core;

public class ThunderscansSearchCompletion : ISearchCompletion
{
    public string ProviderName => "Thunderscans";
    public string ProviderImagePath => "/Images/Providers/thunderscans.png";

    public async Task<IEnumerable<IManwhaSearchResult>> Search(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return [];

        var url = $"https://en-thunderscans.com/?s={searchString}";
        
        using var client = CreateHttpClient();
        var responseString = await client.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseString);
        
        var titles = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tt')]")
            .Select(node => node.InnerText.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        if (titles.Count == 0)
            return [];
        
        var manwhaImages = htmlDoc.DocumentNode.SelectNodes("//img[contains(@class, 'ts-post-image wp-post-image attachment-medium size-medium')]")
            .Select(node => node.GetAttributeValue("src", string.Empty))
            .ToList();
        
        var manwhaResults = new List<IManwhaSearchResult>();
        
        for (var i = 0; i < titles.Count; i++)
        {
            manwhaResults.Add(new ManwhaSearchResult()
            {
                Title = titles.ElementAt(i),
                ImageUrl = manwhaImages.ElementAt(i),
            });
        }
        
        return manwhaResults;
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ManwhaReader");
        return client;
    }
}