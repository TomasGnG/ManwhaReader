using HtmlAgilityPack;

namespace ManwhaReader.Core;

public class ThunderscansSearchCompletion : ISearchCompletion
{
    public string ProviderName => "Thunderscans";
    public string ProviderImagePath => "/Images/Providers/thunderscans.png";

    public async Task<IEnumerable<string>> Search(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return [];

        var url = $"https://en-thunderscans.com/?s={searchString}";
        
        using var client = CreateHttpClient();
        var responseString = await client.GetStringAsync(url);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseString);

        var titleNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'tt')]");

        return titleNodes == null
            ? []
            : titleNodes.Select(node => node.InnerText.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ManwhaReader");
        return client;
    }
}