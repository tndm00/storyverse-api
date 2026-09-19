namespace Content.Application.Queries.Stories.GetViewStats;

public sealed class GetViewStatsQueryHandler : IQueryHandler<GetViewStatsQuery, ViewStatsResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IViewStatsReader _viewStatsReader;

    public GetViewStatsQueryHandler(IStoryRepository storyRepository, IViewStatsReader viewStatsReader)
    {
        _storyRepository = storyRepository;
        _viewStatsReader = viewStatsReader;
    }

    /// <summary>
    /// Builds the admin view statistics: the lifetime total (Postgres counters plus views still
    /// waiting in the Redis buffer), today's and yesterday's totals, and today's most viewed stories.
    /// The daily figures are null when Redis is unavailable rather than a misleading zero.
    /// </summary>
    public async Task<ViewStatsResponseDto> Handle(GetViewStatsQuery request, CancellationToken cancellationToken)
    {
        // Lifetime views already flushed to Postgres, plus what Redis still holds.
        var flushedTotal = await _storyRepository.SumViewCountAsync(cancellationToken);
        var snapshot = await _viewStatsReader.GetSnapshotAsync(
            ApplicationConstants.ViewStatsTopStoriesCount, cancellationToken);

        // Resolve titles/slugs for the ranked story ids, keeping Redis' ranking order.
        var topStories = await BuildTopStoriesAsync(snapshot, cancellationToken);

        return new ViewStatsResponseDto
        {
            TotalViews = flushedTotal + snapshot.PendingStoryViews,
            TodayViews = snapshot.IsAvailable ? snapshot.TodayViews : null,
            YesterdayViews = snapshot.IsAvailable ? snapshot.YesterdayViews : null,
            TrackingSince = snapshot.TrackingSince,
            TopStoriesToday = topStories
        };
    }

    /// <summary>Joins the Redis ranking (story id + views) with story titles/slugs from Postgres.</summary>
    private async Task<IReadOnlyList<TopStoryViewsResponseDto>> BuildTopStoriesAsync(
        ViewStatsSnapshot snapshot, CancellationToken cancellationToken)
    {
        if (snapshot.TopStoriesToday.Count == 0)
        {
            return Array.Empty<TopStoryViewsResponseDto>();
        }

        var viewsByStoryId = snapshot.TopStoriesToday.ToDictionary(x => x.StoryId, x => x.Views);
        var stories = await _storyRepository.GetByIdsInOrderAsync(
            snapshot.TopStoriesToday.Select(x => x.StoryId).ToList(), cancellationToken);

        // Stories deleted since they were ranked are simply omitted by the repository.
        return stories
            .Select(story => new TopStoryViewsResponseDto
            {
                Title = story.Title,
                Slug = story.Slug,
                Views = viewsByStoryId[story.Id]
            })
            .ToList();
    }
}
