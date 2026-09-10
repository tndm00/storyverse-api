namespace Content.Application.Queries.Moderation.GetChapterTitles;

public sealed class GetChapterTitlesQueryHandler
    : IQueryHandler<GetChapterTitlesQuery, IReadOnlyList<ContentTitleEntryDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetChapterTitlesQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<IReadOnlyList<ContentTitleEntryDto>> Handle(
        GetChapterTitlesQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ContentTitleEntryDto>();
        }

        return await _chapterRepository.GetTitlesByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
