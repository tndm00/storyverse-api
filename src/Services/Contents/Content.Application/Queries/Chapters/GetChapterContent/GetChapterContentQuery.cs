namespace Content.Application.Queries.Chapters.GetChapterContent;

/// <summary>
/// Reader view of a single chapter. A published chapter increments the view count.
/// Draft chapters are visible only to the owning author.
/// </summary>
public sealed class GetChapterContentQuery : IQuery<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
