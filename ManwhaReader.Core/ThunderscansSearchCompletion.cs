namespace ManwhaReader.Core;

public class ThunderscansSearchCompletion : ISearchCompletion
{
    public string ProviderName => "Thunderscans";
    
    public async Task<IEnumerable<string>> Search(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return [];
    }
}