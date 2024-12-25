using ManwhaReader.Core.Interfaces;

namespace ManwhaReader.Core;

public class ManwhaSearchResult : IManwhaSearchResult
{
    public required string Title { get; init; }
    public required byte[] ImageData { get; init; }
}