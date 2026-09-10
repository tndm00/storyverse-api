namespace Content.Application.Queries.Chapters.GetChapterReviewCounts;

public sealed class GetChapterReviewCountsQueryHandler
    : IQueryHandler<GetChapterReviewCountsQuery, ChapterReviewCountsResponseDto>
{
    private readonly IChapterRepository _chapterRepository;

    public GetChapterReviewCountsQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<ChapterReviewCountsResponseDto> Handle(
        GetChapterReviewCountsQuery request,
        CancellationToken cancellationToken)
    {
        var (pending, inReview, approved, rejected) =
            await _chapterRepository.GetReviewCountsAsync(cancellationToken);

        return new ChapterReviewCountsResponseDto
        {
            Pending = pending,
            InReview = inReview,
            Approved = approved,
            Rejected = rejected
        };
    }
}
