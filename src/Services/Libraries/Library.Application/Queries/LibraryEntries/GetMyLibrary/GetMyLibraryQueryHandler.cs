namespace Library.Application.Queries.LibraryEntries.GetMyLibrary;

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

    public async Task<PagedResponseDto<LibraryEntryResponseDto>> Handle(
        GetMyLibraryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var shelfStatus = ShelfStatusParser.ParseFilter(request.ShelfStatus);

        var (items, totalCount) = await _libraryEntryRepository.GetPagedAsync(
            userId, shelfStatus, pageNumber, pageSize, cancellationToken);

        var entries = items.Select(LibraryDtoMapper.ToDto).ToArray();

        return PagedResponseDto<LibraryEntryResponseDto>.Create(entries, pageNumber, pageSize, totalCount);
    }
}
