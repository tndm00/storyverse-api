namespace Community.Application.Queries.Comments.GetCommentExcerpts;

/// <summary>Handles <see cref="GetCommentExcerptsQuery"/>: batch-resolves comment public ids to short content excerpts for internal service-to-service calls.</summary>
public sealed class GetCommentExcerptsQueryHandler
    : IQueryHandler<GetCommentExcerptsQuery, IReadOnlyList<CommentExcerptEntryDto>>
{
    private const int MaxExcerptLength = 140;

    private readonly ICommentRepository _commentRepository;

    /// <summary>Creates the handler with its repository dependency.</summary>
    public GetCommentExcerptsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    /// <summary>Looks up the requested comments by public id and returns each one's trimmed excerpt.</summary>
    public async Task<IReadOnlyList<CommentExcerptEntryDto>> Handle(
        GetCommentExcerptsQuery request, CancellationToken cancellationToken)
    {
        // Nothing to look up for an empty id set.
        if (request.Ids.Count == 0)
        {
            return Array.Empty<CommentExcerptEntryDto>();
        }

        // Batch-fetch the comments and map each to its excerpt.
        var comments = await _commentRepository.GetByPublicIdsAsync(request.Ids, cancellationToken);

        return comments
            .Select(c => new CommentExcerptEntryDto { Id = c.PublicId, Excerpt = Excerpt(c.Content) })
            .ToArray();
    }

    /// <summary>Trims the content and truncates it to <see cref="MaxExcerptLength"/>, appending an ellipsis when cut short.</summary>
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
