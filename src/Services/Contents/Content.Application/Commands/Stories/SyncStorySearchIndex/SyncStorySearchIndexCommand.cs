namespace Content.Application.Commands.Stories.SyncStorySearchIndex;

/// <summary>
/// Background-job tick: syncs every story/chapter changed since the last run
/// into Elasticsearch. Sent on a timer by
/// Content.Api.BackgroundServices.StorySearchIndexSyncBackgroundService — no
/// write-path command handler calls <see cref="Interfaces.IStorySearchService"/>
/// directly, this command is the sole writer, fully decoupled from business
/// logic.
/// </summary>
public sealed class SyncStorySearchIndexCommand : ICommand<SyncStorySearchIndexResultDto>;

public sealed class SyncStorySearchIndexResultDto
{
    public int ChangedStoryCount { get; init; }
}
