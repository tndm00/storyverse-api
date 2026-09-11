namespace Content.Application.Dtos;

/// <summary>
/// Internal request body from the Community service: the recomputed visible
/// comment count for one chapter, sent after a comment write (add/reply/
/// delete/visibility change) commits. Best-effort sync, not part of the
/// Community write transaction.
/// </summary>
public sealed class ChapterCommentCountRequestDto
{
    public int Count { get; init; }
}

/// <summary>Internal response: the chapter's public id and the stored comment count.</summary>
public sealed class ChapterCommentCountResponseDto
{
    public Guid Id { get; init; }

    public int CommentCount { get; init; }
}
