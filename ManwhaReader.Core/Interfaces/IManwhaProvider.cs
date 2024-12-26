namespace ManwhaReader.Core.Interfaces;

public interface IManwhaProvider
{
    string Name { get; }
    string ImageUrl { get; }
    
    Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true);
}