namespace Content.Application.Queries.Chapters.GetPendingReviewChapters;

public sealed class GetPendingReviewChaptersQueryHandler
    : IQueryHandler<GetPendingReviewChaptersQuery, PagedResponseDto<PendingReviewChapterResponseDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetPendingReviewChaptersQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    /// <summary>
    /// Returns a paged, filterable cross-story queue of chapters pending moderation review.
    /// </summary>
    public async Task<PagedResponseDto<PendingReviewChapterResponseDto>> Handle(
        GetPendingReviewChaptersQuery request,
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

        // Only PendingReview/InReview are valid queue statuses; anything else means no filter.
        ChapterStatus? status =
            Enum.TryParse<ChapterStatus>(request.Status, ignoreCase: true, out var parsed)
            && parsed is ChapterStatus.PendingReview or ChapterStatus.InReview
                ? parsed
                : null;

        var (items, totalCount) = await _chapterRepository.GetPendingReviewAsync(
            status, request.Keyword, pageNumber, pageSize, cancellationToken);

        // Map chapter/story pairs to the queue response DTO.
        var dtos = items
            .Select(x => ContentDtoMapper.ToPendingReviewDto(x.Chapter, x.Story))
            .ToArray();

        return PagedResponseDto<PendingReviewChapterResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
