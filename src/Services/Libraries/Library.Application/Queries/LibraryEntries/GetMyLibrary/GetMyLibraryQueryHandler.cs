namespace Library.Application.Queries.LibraryEntries.GetMyLibrary;

/// <summary>
/// Handles <see cref="GetMyLibraryQuery"/> by returning a paged, optionally
/// shelf-filtered listing of the authenticated caller's library.
/// </summary>
public sealed class GetMyLibraryQueryHandler
    : IQueryHandler<GetMyLibraryQuery, PagedResponseDto<LibraryEntryResponseDto>>
{
    private readonly ILibraryEntryRepository _libraryEntryRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetMyLibraryQueryHandler(
        ILibraryEntryRepository libraryEntryRepository,
        ICurrentUserContext currentUser)
    {
        _libraryEntryRepository = libraryEntryRepository;
        _currentUser = currentUser;
    }

    /// <summary>Fetches one normalized, optionally shelf-filtered page of the caller's library.</summary>
    public async Task<PagedResponseDto<LibraryEntryResponseDto>> Handle(
        GetMyLibraryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        // Clamp paging parameters to valid, bounded values.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var shelfStatus = ShelfStatusParser.ParseFilter(request.ShelfStatus);

        // Fetch the page and map entities to response DTOs.
        var (items, totalCount) = await _libraryEntryRepository.GetPagedAsync(
            userId, shelfStatus, pageNumber, pageSize, cancellationToken);

        var entries = items.Select(LibraryDtoMapper.ToDto).ToArray();

        return PagedResponseDto<LibraryEntryResponseDto>.Create(entries, pageNumber, pageSize, totalCount);
    }
}
