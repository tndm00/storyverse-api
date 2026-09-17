namespace Content.Application.Queries.Moderation.GetChapterTitles;

public sealed class GetChapterTitlesQueryHandler
    : IQueryHandler<GetChapterTitlesQuery, IReadOnlyList<ContentTitleEntryDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetChapterTitlesQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    /// <summary>Batch-resolves chapter titles for the given public ids.</summary>
    public async Task<IReadOnlyList<ContentTitleEntryDto>> Handle(
        GetChapterTitlesQuery request, CancellationToken cancellationToken)
    {
        // Skip the round-trip entirely when there is nothing to look up.
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ContentTitleEntryDto>();
        }

        return await _chapterRepository.GetTitlesByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
