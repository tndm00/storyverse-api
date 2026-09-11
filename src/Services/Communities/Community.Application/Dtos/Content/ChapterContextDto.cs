namespace Community.Application.Dtos;

/// <summary>
/// Story context for one chapter, resolved from the Content service — enough to
/// render a "story / chapter" link next to a comment without a second round-trip.
/// </summary>
public sealed class ChapterContextDto
{
    public Guid ChapterId { get; init; }

    public string ChapterTitle { get; init; }

    public Guid StoryId { get; init; }

    public string StorySlug { get; init; }

    public string StoryTitle { get; init; }
}
