namespace ManwhaReader.Core;

public interface ISearchCompletion
{
    string ProviderName { get; }
    string ProviderImagePath { get; }
    
    Task<IEnumerable<string>> Search(string searchString);
}