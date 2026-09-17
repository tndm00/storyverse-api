namespace Content.Application.Queries.Stories.GetStoryStatusCounts;

public sealed class GetStoryStatusCountsQueryHandler
    : IQueryHandler<GetStoryStatusCountsQuery, StoryStatusCountsResponseDto>
{
    private readonly IStoryRepository _storyRepository;

    public GetStoryStatusCountsQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    /// <summary>Returns the total story count plus a per-status breakdown for the admin dashboard.</summary>
    public async Task<StoryStatusCountsResponseDto> Handle(
        GetStoryStatusCountsQuery request,
        CancellationToken cancellationToken)
    {
        var counts = await _storyRepository.CountByStatusAsync(cancellationToken);

        return new StoryStatusCountsResponseDto
        {
            Total = counts.Values.Sum(),
            ByStatus = counts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value)
        };
    }
}
