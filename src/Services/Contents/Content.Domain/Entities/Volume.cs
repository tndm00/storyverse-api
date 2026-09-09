namespace Content.Domain.Entities;

/// <summary>
/// Optional grouping of chapters within a story (a "tập"/arc). A chapter may
/// belong to no volume.
/// </summary>
public sealed class Volume : BaseEntity
{
    public long StoryId { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }
}
