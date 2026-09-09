namespace Library.Application.Queries.ReadingProgresses.GetContinueReading;

public sealed class GetContinueReadingQueryHandler
    : IQueryHandler<GetContinueReadingQuery, PagedResponseDto<ReadingProgressResponseDto>>
{
    private readonly IReadingProgressRepository _readingProgressRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetContinueReadingQueryHandler(
        IReadingProgressRepository readingProgressRepository,
        ICurrentUserContext currentUser)
    {
        _readingProgressRepository = readingProgressRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResponseDto<ReadingProgressResponseDto>> Handle(
        GetContinueReadingQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var (items, totalCount) = await _readingProgressRepository.GetRecentAsync(
            userId, pageNumber, pageSize, cancellationToken);

        var rows = items.Select(LibraryDtoMapper.ToDto).ToArray();

        return PagedResponseDto<ReadingProgressResponseDto>.Create(rows, pageNumber, pageSize, totalCount);
    }
}
