namespace Content.Application.Queries.Chapters.GetReviewedChapters;

public sealed class GetReviewedChaptersQueryHandler
    : IQueryHandler<GetReviewedChaptersQuery, PagedResponseDto<PendingReviewChapterResponseDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetReviewedChaptersQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    /// <summary>
    /// Returns a paged, filterable list of chapters a moderator has already approved or rejected.
    /// </summary>
    public async Task<PagedResponseDto<PendingReviewChapterResponseDto>> Handle(
        GetReviewedChaptersQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize paging inputs to safe bounds.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        // The queue holds chapters only; a non-chapter type filter matches nothing.
        if (!string.IsNullOrWhiteSpace(request.Type)
            && !string.Equals(request.Type, "Chapter", StringComparison.OrdinalIgnoreCase))
        {
            return PagedResponseDto<PendingReviewChapterResponseDto>.Create(
                Array.Empty<PendingReviewChapterResponseDto>(), pageNumber, pageSize, 0);
        }

        // Any status other than "Rejected" defaults to "Approved".
        var actionType = string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase)
            ? ChapterReviewActionType.Rejected
            : ChapterReviewActionType.Approved;

        var (items, totalCount) = await _chapterRepository.GetReviewedAsync(
            actionType, request.Keyword, pageNumber, pageSize, cancellationToken);

        // Map chapter/story pairs to the queue response DTO.
        var dtos = items
            .Select(x => ContentDtoMapper.ToPendingReviewDto(x.Chapter, x.Story))
            .ToArray();

        return PagedResponseDto<PendingReviewChapterResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
