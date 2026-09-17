namespace Community.Application.Queries.Comments.GetChapterComments;

/// <summary>Handles <see cref="GetChapterCommentsQuery"/>: returns a chapter's visible comments enriched with author display names.</summary>
public sealed class GetChapterCommentsQueryHandler
    : IQueryHandler<GetChapterCommentsQuery, PagedResponseDto<CommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserDirectoryClient _userDirectory;

    /// <summary>Creates the handler with its repository and user directory dependencies.</summary>
    public GetChapterCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserDirectoryClient userDirectory)
    {
        _commentRepository = commentRepository;
        _userDirectory = userDirectory;
    }

    /// <summary>Fetches the chapter's visible comments page and attaches each author's display name.</summary>
    public async Task<PagedResponseDto<CommentResponseDto>> Handle(
        GetChapterCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        // Load the visible comments for this chapter's page.
        var (items, totalCount) = await _commentRepository.GetVisibleByChapterAsync(
            request.ChapterId, pageNumber, pageSize, cancellationToken);

        // One batched lookup for every distinct author on the page.
        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(c => c.AuthorUserId), cancellationToken);

        // Map to DTOs, attaching the resolved author name when available.
        var dtos = items
            .Select(c => CommunityDtoMapper.ToDto(
                c, names.TryGetValue(c.AuthorUserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<CommentResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
