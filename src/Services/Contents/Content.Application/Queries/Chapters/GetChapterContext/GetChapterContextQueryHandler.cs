namespace Content.Application.Queries.Chapters.GetChapterContext;

public sealed class GetChapterContextQueryHandler
    : IQueryHandler<GetChapterContextQuery, IReadOnlyList<ChapterContextEntryDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetChapterContextQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    /// <summary>Batch-resolves chapter context entries for the given public ids.</summary>
    public async Task<IReadOnlyList<ChapterContextEntryDto>> Handle(
        GetChapterContextQuery request, CancellationToken cancellationToken)
    {
        // Skip the round-trip entirely when there is nothing to look up.
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ChapterContextEntryDto>();
        }

        return await _chapterRepository.GetContextByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
