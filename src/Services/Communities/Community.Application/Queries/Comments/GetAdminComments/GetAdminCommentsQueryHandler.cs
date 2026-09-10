namespace Community.Application.Queries.Comments.GetAdminComments;

public sealed class GetAdminCommentsQueryHandler
    : IQueryHandler<GetAdminCommentsQuery, PagedResponseDto<CommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserDirectoryClient _userDirectory;

    public GetAdminCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserDirectoryClient userDirectory)
    {
        _commentRepository = commentRepository;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResponseDto<CommentResponseDto>> Handle(
        GetAdminCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var status = ParseStatus(request.Status);
        var sortAscending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        var (items, totalCount) = await _commentRepository.SearchAsync(
            request.ChapterId,
            request.AuthorUserId,
            status,
            request.Keyword,
            sortAscending,
            pageNumber,
            pageSize,
            cancellationToken);

        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(c => c.AuthorUserId), cancellationToken);

        var dtos = items
            .Select(c => CommunityDtoMapper.ToDto(
                c, names.TryGetValue(c.AuthorUserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<CommentResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }

    private static CommentStatus? ParseStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (Enum.TryParse<CommentStatus>(status, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new BadRequestException(ApplicationErrorConstants.InvalidCommentStatusFilter);
    }
}
