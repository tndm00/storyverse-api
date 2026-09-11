namespace Community.Application.Queries.Comments.GetRecentComments;

public sealed class GetRecentCommentsQueryHandler
    : IQueryHandler<GetRecentCommentsQuery, IReadOnlyList<RecentCommentEntryDto>>
{
    private const int MaxExcerptLength = 140;
    private const int DefaultLimit = 15;
    private const int MaxLimit = 50;

    private readonly ICommentRepository _commentRepository;
    private readonly IUserDirectoryClient _userDirectory;
    private readonly IContentChapterContextClient _chapterContext;

    public GetRecentCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserDirectoryClient userDirectory,
        IContentChapterContextClient chapterContext)
    {
        _commentRepository = commentRepository;
        _userDirectory = userDirectory;
        _chapterContext = chapterContext;
    }

    public async Task<IReadOnlyList<RecentCommentEntryDto>> Handle(
        GetRecentCommentsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit <= 0 ? DefaultLimit : request.Limit, 1, MaxLimit);

        var comments = await _commentRepository.GetRecentVisibleAsync(limit, cancellationToken);
        if (comments.Count == 0)
        {
            return Array.Empty<RecentCommentEntryDto>();
        }

        // Two batched lookups (author names, chapter/story context) rather than
        // N+1 calls. Both are best-effort: a failure leaves the corresponding
        // fields null on every affected item instead of failing the request.
        var names = await _userDirectory.GetDisplayNamesAsync(
            comments.Select(c => c.AuthorUserId), cancellationToken);
        var contexts = await _chapterContext.GetContextAsync(
            comments.Select(c => c.ChapterId), cancellationToken);

        return comments
            .Select(c =>
            {
                contexts.TryGetValue(c.ChapterId, out var context);
                names.TryGetValue(c.AuthorUserId, out var authorName);

                return new RecentCommentEntryDto
                {
                    CommentId = c.PublicId,
                    ChapterId = c.ChapterId,
                    Content = Excerpt(c.Content),
                    AuthorUserId = c.AuthorUserId,
                    AuthorDisplayName = authorName,
                    CreatedAt = c.CreatedAt,
                    StoryId = context?.StoryId,
                    StorySlug = context?.StorySlug,
                    StoryTitle = context?.StoryTitle,
                    ChapterTitle = context?.ChapterTitle
                };
            })
            .ToArray();
    }

    private static string Excerpt(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        return trimmed.Length <= MaxExcerptLength ? trimmed : trimmed[..MaxExcerptLength] + "…";
    }
}
