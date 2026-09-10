namespace Content.Application.Queries.Moderation.GetStoryTitles;

public sealed class GetStoryTitlesQueryHandler
    : IQueryHandler<GetStoryTitlesQuery, IReadOnlyList<ContentTitleEntryDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetStoryTitlesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<IReadOnlyList<ContentTitleEntryDto>> Handle(
        GetStoryTitlesQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ContentTitleEntryDto>();
        }

        return await _storyRepository.GetTitlesByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
