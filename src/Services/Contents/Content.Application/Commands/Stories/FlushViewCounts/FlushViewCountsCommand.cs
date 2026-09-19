namespace Content.Application.Commands.Stories.FlushViewCounts;

/// <summary>
/// Background-job tick: moves the view counts buffered in Redis into the Postgres
/// <c>ViewCount</c> columns. Sent on a timer by
/// Content.Api.BackgroundServices.ViewCountFlushBackgroundService.
/// </summary>
public sealed class FlushViewCountsCommand : ICommand<FlushViewCountsResultDto>;

public sealed class FlushViewCountsResultDto
{
    public int StoryCount { get; init; }

    public int ChapterCount { get; init; }
}
