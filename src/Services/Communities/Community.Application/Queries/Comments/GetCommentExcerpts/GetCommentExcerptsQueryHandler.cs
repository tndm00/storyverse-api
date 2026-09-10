namespace Community.Application.Queries.Comments.GetCommentExcerpts;

public sealed class GetCommentExcerptsQueryHandler
    : IQueryHandler<GetCommentExcerptsQuery, IReadOnlyList<CommentExcerptEntryDto>>
{
    private const int MaxExcerptLength = 140;

    private readonly ICommentRepository _commentRepository;

    public GetCommentExcerptsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<CommentExcerptEntryDto>> Handle(
        GetCommentExcerptsQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Array.Empty<CommentExcerptEntryDto>();
        }

        var comments = await _commentRepository.GetByPublicIdsAsync(request.Ids, cancellationToken);

        return comments
            .Select(c => new CommentExcerptEntryDto { Id = c.PublicId, Excerpt = Excerpt(c.Content) })
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
