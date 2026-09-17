namespace Content.Application.Queries.Moderation.GetStoryTitles;

public sealed class GetStoryTitlesQueryHandler
    : IQueryHandler<GetStoryTitlesQuery, IReadOnlyList<ContentTitleEntryDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetStoryTitlesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    /// <summary>Batch-resolves story titles for the given public ids.</summary>
    public async Task<IReadOnlyList<ContentTitleEntryDto>> Handle(
        GetStoryTitlesQuery request, CancellationToken cancellationToken)
    {
        // Skip the round-trip entirely when there is nothing to look up.
        if (request.Ids.Count == 0)
        {
            return Array.Empty<ContentTitleEntryDto>();
        }

        return await _storyRepository.GetTitlesByPublicIdsAsync(request.Ids, cancellationToken);
    }
}
