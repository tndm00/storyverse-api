namespace Content.Application.Commands.Chapters.PublishDueChapters;

/// <summary>
/// Publishes every <see cref="Content.Domain.Enums.ChapterStatus.Scheduled"/>
/// chapter whose scheduled time has arrived. Invoked on a timer by the Content
/// host's background publisher, not from an HTTP endpoint. Idempotent and safe
/// to run concurrently from multiple instances.
/// </summary>
public sealed class PublishDueChaptersCommand : ICommand<PublishDueChaptersResultDto>
{
    /// <summary>Upper bound on chapters published in this pass.</summary>
    public int BatchSize { get; init; } = 100;
}

/// <summary>Outcome of one publisher pass.</summary>
public sealed class PublishDueChaptersResultDto
{
    public int PublishedCount { get; init; }

    /// <summary>Chapters that were due this pass (may exceed <see cref="PublishedCount"/> if some were claimed by another instance).</summary>
    public int DueCount { get; init; }
}
