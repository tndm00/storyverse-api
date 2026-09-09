namespace Content.Domain.Entities;

/// <summary>
/// Join row between a story and a free-form tag.
/// </summary>
public sealed class StoryTag
{
    public long StoryId { get; set; }

    public long TagId { get; set; }

    public Tag Tag { get; set; }
}
