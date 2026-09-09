namespace Library.Application.Dtos;

/// <summary>Payload written whenever a chapter is opened, to record where the reader is.</summary>
public sealed class UpsertReadingProgressRequestDto
{
    public Guid LastChapterId { get; init; }

    /// <summary>Optional scroll position within the chapter, 0–100.</summary>
    public decimal? ScrollPercent { get; init; }
}

/// <summary>The reader's last position in a single story.</summary>
public sealed class ReadingProgressResponseDto
{
    public Guid Id { get; init; }

    public Guid StoryId { get; init; }

    public Guid LastChapterId { get; init; }

    public decimal? ScrollPercent { get; init; }

    public DateTime LastReadAt { get; init; }
}
