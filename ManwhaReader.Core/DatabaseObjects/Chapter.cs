using DevExpress.Xpo;

namespace ManwhaReader.Core.DatabaseObjects;

[Persistent("Chapter")]
public class Chapter : XPBaseObject
{
    public Chapter(Session session) : base(session) { }
    
    [Key(true), Persistent("ChapterID")]
    public Guid ChapterId
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(ChapterId), value);
    }
    
    [NonPersistent]
    public string Name => string.IsNullOrWhiteSpace(ChapterTitle) ? $"Chapter {Number}" : $"Chapter {Number} | {ChapterTitle}";

    [Persistent("ChapterTitle")]
    public string? ChapterTitle
    {
        get => GetPropertyValue<string?>();
        set => SetPropertyValue(nameof(ChapterTitle), value);
    }

    [Persistent("Number")]
    public double Number
    {
        get => GetPropertyValue<double>();
        set => SetPropertyValue(nameof(Number), value);
    }

    [Persistent("AlreadyRead")]
    public bool AlreadyRead
    {
        get => GetPropertyValue<bool>();
        set => SetPropertyValue(nameof(AlreadyRead), value);
    }
    
    [Persistent("ManwhaID")]
    public Guid Manwha
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(Manwha), value);
    }
    
    [NonPersistent]
    public IEnumerable<ChapterImageUrl> ImageUrls => Session.DefaultSession.Query<ChapterImageUrl>().Where(x => x.Chapter == ChapterId).OrderBy(x => x.UrlNumber);
}
