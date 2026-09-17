namespace Content.Application.Commands.Stories.ReindexAllStories;

/// <summary>
/// One-time/backfill: indexes every non-Draft story into Elasticsearch. See
/// <c>POST /v1/stories/admin/reindex-search</c> — run this once after the
/// elasticsearch container is confirmed healthy, before flipping
/// <c>Elasticsearch:SearchReadEnabled</c> on.
/// </summary>
public sealed class ReindexAllStoriesCommand : ICommand<ReindexAllStoriesResultDto>;

public sealed class ReindexAllStoriesResultDto
{
    public int StoryCount { get; init; }
}
