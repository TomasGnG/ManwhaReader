namespace ManwhaReader.Core;

public class ManwhaSearchResult : IManwhaSearchResult
{
    public required string ImageUrl { get; init; }
    public required string Title { get; init; }
}