using DevExpress.Xpo;

namespace ManwhaReader.Core.DatabaseObjects;

[Persistent("ChapterImageUrl")]
public class ChapterImageUrl : XPBaseObject
{
    public ChapterImageUrl(Session session) : base(session) { }

    [Key(true), Persistent("ChapterImageUrlId")]
    public Guid ChapterImageUrlId
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(ChapterImageUrlId), value);
    }

    [Persistent("UrlNumber")]
    public int UrlNumber
    {
        get => GetPropertyValue<int>();
        set => SetPropertyValue(nameof(UrlNumber), value);
    }

    [Persistent("UrlLastChanged")]
    public DateTime UrlLastChanged
    {
        get => GetPropertyValue<DateTime>();
        set => SetPropertyValue(nameof(UrlLastChanged), value);
    }

    [Persistent("Url")]
    public string Url
    {
        get => GetPropertyValue<string>();
        set
        {
            if(!string.Equals(Url, value))
                SetPropertyValue(nameof(Url), value);
            
            UrlLastChanged = DateTime.Now;
        }
    }
    
    [Persistent("ChapterID")]
    public Guid Chapter
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(Chapter), value);
    }
}