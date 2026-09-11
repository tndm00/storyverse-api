namespace Content.Application.Queries.Chapters.GetChapterContext;

public sealed class GetChapterContextQueryHandler
    : IQueryHandler<GetChapterContextQuery, IReadOnlyList<ChapterContextEntryDto>>
{
    private readonly IChapterRepository _chapterRepository;

    public GetChapterContextQueryHandler(IChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<IReadOnlyList<ChapterContextEntryDto>> Handle(
        GetChapterContextQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ChapterContextEntryDto>();
        }

        return await _chapterRepository.GetContextByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
