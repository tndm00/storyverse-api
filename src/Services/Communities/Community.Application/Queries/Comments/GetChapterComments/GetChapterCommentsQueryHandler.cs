namespace Community.Application.Queries.Comments.GetChapterComments;

public sealed class GetChapterCommentsQueryHandler
    : IQueryHandler<GetChapterCommentsQuery, PagedResponseDto<CommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserDirectoryClient _userDirectory;

    public GetChapterCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserDirectoryClient userDirectory)
    {
        _commentRepository = commentRepository;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResponseDto<CommentResponseDto>> Handle(
        GetChapterCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _commentRepository.GetVisibleByChapterAsync(
            request.ChapterId, pageNumber, pageSize, cancellationToken);

        // One batched lookup for every distinct author on the page.
        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(c => c.AuthorUserId), cancellationToken);

        var dtos = items
            .Select(c => CommunityDtoMapper.ToDto(
                c, names.TryGetValue(c.AuthorUserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<CommentResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
