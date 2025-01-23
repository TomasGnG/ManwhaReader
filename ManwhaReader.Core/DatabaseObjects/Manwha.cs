using DevExpress.Xpo;

namespace ManwhaReader.Core.DatabaseObjects;

[Persistent("Manwha")]
public class Manwha : XPBaseObject
{
    public Manwha(Session session) : base(session) { }

    [Key(true), Persistent("ManwhaID")]
    public Guid ManwhaId
    {
        get => GetPropertyValue<Guid>();
        set => SetPropertyValue(nameof(ManwhaId), value);
    }

    [Persistent("ThumbnailUrl")]
    public string ThumbnailUrl
    {
        get => GetPropertyValue<string>();
        set => SetPropertyValue(nameof(ThumbnailUrl), value);
    }
    
    [Persistent("Name")]
    public string Name
    {
        get => GetPropertyValue<string>();
        set => SetPropertyValue(nameof(Name), value);
    }

    [Persistent("Description")]
    public string? Description
    {
        get => GetPropertyValue<string?>();
        set => SetPropertyValue(nameof(Description), value);
    }

    [Persistent("Status")]
    public string? Status
    {
        get => GetPropertyValue<string?>();
        set => SetPropertyValue(nameof(Status), value);
    }

    [Persistent("Tags")]
    public string Tags
    {
        get => GetPropertyValue<string>();
        set => SetPropertyValue(nameof(Tags), value);
    }
    
    public IEnumerable<string> TagsAsEnumerable() => Tags.Split(';');

    [Persistent("LastReadChapterID")]
    [NoForeignKey]
    public Chapter LastReadChapter
    {
        get => GetPropertyValue<Chapter>();
        set => SetPropertyValue(nameof(LastReadChapter), value);
    }

    [NonPersistent]
    public IEnumerable<Chapter> Chapters => Session.DefaultSession.Query<Chapter>().Where(x => x.Manwha == ManwhaId);
}