using ManwhaReader.Core.DatabaseObjects;

namespace ManwhaReader.Core.Interfaces;

public interface IManwhaProvider
{
    string Name { get; }
    string ImageUrl { get; }
    
    Task<IEnumerable<IManwhaSearchResult>> Search(string searchQuery, bool loadImages = true);
    Task<Manwha> GetManwhaByTitle(string title);
    Task<IEnumerable<Chapter>> GetChaptersByManwhaTitle(string title);
    Task<IEnumerable<ChapterImageUrl>> GetChapterImageUrls(string manwhaTitle, double chapterNumber);
}