namespace Content.Application.Commands.Chapters.CreateChapter;

/// <summary>
/// Creates a chapter in a story. Owner only. Starts as a draft unless
/// <see cref="PublishImmediately"/> is set.
/// </summary>
public sealed class CreateChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid StoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Sort position within the story. Leave at <c>0</c> to append after the last
    /// chapter; pass a fractional value to insert between two existing chapters.
    /// </summary>
    public decimal OrderIndex { get; init; }

    public string Content { get; init; } = string.Empty;

    public Guid? VolumeId { get; init; }

    /// <summary>Publish the chapter in the same call (skips the separate publish step).</summary>
    public bool PublishImmediately { get; init; }
}
