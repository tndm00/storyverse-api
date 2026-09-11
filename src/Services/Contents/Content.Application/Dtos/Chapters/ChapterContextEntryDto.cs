namespace Content.Application.Dtos;

/// <summary>
/// Internal batch lookup entry: a chapter's title plus its parent story's id,
/// slug and title — enough for another service to render a "story / chapter"
/// link without a second round-trip. Unknown ids are omitted by the lookup.
/// </summary>
public sealed class ChapterContextEntryDto
{
    public Guid ChapterId { get; init; }

    public string ChapterTitle { get; init; }

    public Guid StoryId { get; init; }

    public string StorySlug { get; init; }

    public string StoryTitle { get; init; }
}
