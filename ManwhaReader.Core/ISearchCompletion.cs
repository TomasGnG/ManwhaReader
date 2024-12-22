namespace ManwhaReader.Core;

public interface ISearchCompletion
{
    string ProviderName { get; }
    
    Task<IEnumerable<string>> Search(string searchString);
}