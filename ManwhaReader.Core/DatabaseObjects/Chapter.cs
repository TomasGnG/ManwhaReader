using DevExpress.Xpo;

namespace ManwhaReader.Core.DatabaseObjects;

[Persistent("Chapter")]
public class Chapter : PersistentBase
{
    protected Chapter(Session session) : base(session) { }
    
    [Key(true), Persistent("ChapterID")]
    public Guid ChapterId
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(ChapterId), value);
    }

    [Persistent("Number")]
    public int Number
    {
        get => GetPropertyValue<int>();
        set => SetPropertyValue(nameof(Number), value);
    }

    [Persistent("AlreadyRead")]
    public bool AlreadyRead
    {
        get => GetPropertyValue<bool>();
        set => SetPropertyValue(nameof(AlreadyRead), value);
    }

    public List<string> ImageUrls
    {
        get => GetPropertyValue<List<string>>();
        set => SetPropertyValue(nameof(ImageUrls), value);
    }
    
    [Association]
    public Manwha Manwha
    {
        get => GetPropertyValue<Manwha>();
        set => SetPropertyValue(nameof(Manwha), value);
    }
}
