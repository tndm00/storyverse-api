namespace Content.Application.Queries.Chapters.GetReviewedChapters;

public sealed class GetReviewedChaptersQueryHandler
    : IQueryHandler<GetReviewedChaptersQuery, PagedResponseDto<PendingReviewChapterResponseDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetReviewedChaptersQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<PagedResponseDto<PendingReviewChapterResponseDto>> Handle(
        GetReviewedChaptersQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        if (!string.IsNullOrWhiteSpace(request.Type)
            && !string.Equals(request.Type, "Chapter", StringComparison.OrdinalIgnoreCase))
        {
            return PagedResponseDto<PendingReviewChapterResponseDto>.Create(
                Array.Empty<PendingReviewChapterResponseDto>(), pageNumber, pageSize, 0);
        }

        var actionType = string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? ChapterReviewActionType.Rejected
            : ChapterReviewActionType.Approved;

        var (items, totalCount) = await _chapterRepository.GetReviewedAsync(
            actionType, request.Keyword, pageNumber, pageSize, cancellationToken);

        var dtos = items
            .Select(x => ContentDtoMapper.ToPendingReviewDto(x.Chapter, x.Story))
            .ToArray();

        return PagedResponseDto<PendingReviewChapterResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
