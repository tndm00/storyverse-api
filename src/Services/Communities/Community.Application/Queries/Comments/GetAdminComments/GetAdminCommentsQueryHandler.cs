namespace Community.Application.Queries.Comments.GetAdminComments;

/// <summary>Handles <see cref="GetAdminCommentsQuery"/>: cross-chapter comment moderation listing with author display names.</summary>
public sealed class GetAdminCommentsQueryHandler
    : IQueryHandler<GetAdminCommentsQuery, PagedResponseDto<CommentResponseDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserDirectoryClient _userDirectory;

    /// <summary>Creates the handler with its repository and user directory dependencies.</summary>
    public GetAdminCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserDirectoryClient userDirectory)
    {
        _commentRepository = commentRepository;
        _userDirectory = userDirectory;
    }

    /// <summary>Searches comments across chapters by status, author, keyword and sort order, then enriches each page with author display names.</summary>
    public async Task<PagedResponseDto<CommentResponseDto>> Handle(
        GetAdminCommentsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize paging and parse the optional status/sort filters.
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var status = ParseStatus(request.Status);
        var sortAscending = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        // Search the repository with all filters applied.
        var (items, totalCount) = await _commentRepository.SearchAsync(
            request.ChapterId,
            request.AuthorUserId,
            status,
            request.Keyword,
            sortAscending,
            pageNumber,
            pageSize,
            cancellationToken);

        // Batch-resolve author display names for the current page.
        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(c => c.AuthorUserId), cancellationToken);

        // Map to DTOs, attaching the resolved author name when available.
        var dtos = items
            .Select(c => CommunityDtoMapper.ToDto(
                c, names.TryGetValue(c.AuthorUserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<CommentResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }

    /// <summary>Parses the status filter string into a <see cref="CommentStatus"/>, treating blank/"all" as "no filter".</summary>
    private static CommentStatus? ParseStatus(string status)
    {
        // Blank or "all" means every status.
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Otherwise the value must match a known status.
        if (Enum.TryParse<CommentStatus>(status, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new BadRequestException(ApplicationErrorConstants.InvalidCommentStatusFilter);
    }
}
