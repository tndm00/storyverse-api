namespace Content.Application.Queries.Chapters.GetPendingReviewChapters;

public sealed class GetPendingReviewChaptersQueryHandler
    : IQueryHandler<GetPendingReviewChaptersQuery, PagedResponseDto<PendingReviewChapterResponseDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetPendingReviewChaptersQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<PagedResponseDto<PendingReviewChapterResponseDto>> Handle(
        GetPendingReviewChaptersQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        ChapterStatus? status = Enum.TryParse<ChapterStatus>(request.Status, ignoreCase: true, out var parsed)
            ? parsed
            : null;

        var (items, totalCount) = await _chapterRepository.GetPendingReviewAsync(
            status, pageNumber, pageSize, cancellationToken);

        var dtos = items
            .Select(x => ContentDtoMapper.ToPendingReviewDto(x.Chapter, x.Story))
            .ToArray();

        return PagedResponseDto<PendingReviewChapterResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
