namespace Community.Application.Dtos;

/// <summary>
/// One entry in the cross-platform "recently commented" feed
/// (<c>GET /v1/comments/recent</c>). Story/chapter fields are best-effort:
/// null when the Content-side enrichment lookup could not resolve them.
/// </summary>
public sealed class RecentCommentEntryDto
{
    public Guid CommentId { get; init; }

    public Guid ChapterId { get; init; }

    public string Content { get; init; }

    public long AuthorUserId { get; init; }

    public string AuthorDisplayName { get; init; }

    public DateTime CreatedAt { get; init; }

    public Guid? StoryId { get; init; }

    public string StorySlug { get; init; }

    public string StoryTitle { get; init; }

    public string ChapterTitle { get; init; }
}
