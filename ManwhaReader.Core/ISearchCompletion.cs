namespace ManwhaReader.Core;

public interface ISearchCompletion
{
    string ProviderName { get; }
    string ProviderImagePath { get; }
    
    Task<IEnumerable<IManwhaSearchResult>> Search(string searchString);
}