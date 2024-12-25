namespace ManwhaReader.Core.Interfaces;

public interface IManwhaSearchResult
{
    public string Title { get; }
    public byte[] ImageData { get; }
}