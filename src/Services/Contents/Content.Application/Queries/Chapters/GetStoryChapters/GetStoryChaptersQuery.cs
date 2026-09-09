namespace Content.Application.Queries.Chapters.GetStoryChapters;

/// <summary>
/// Table of contents for a story. The owner sees every chapter (including drafts);
/// everyone else sees only published chapters.
/// </summary>
public sealed class GetStoryChaptersQuery : IQuery<IReadOnlyList<ChapterSummaryResponseDto>>
{
    public Guid StoryId { get; init; }
}
