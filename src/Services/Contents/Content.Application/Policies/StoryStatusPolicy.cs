namespace Content.Application.Policies;

/// <summary>
/// Allowed <see cref="StoryStatus"/> transitions, per product-workflow-context.md
/// section 7. <c>Draft → Ongoing</c> is intentionally excluded here: it happens
/// automatically when the first chapter is published, never by a manual request.
/// </summary>
public static class StoryStatusPolicy
{
    private static readonly HashSet<(StoryStatus From, StoryStatus To)> AllowedManualTransitions = new()
    {
        (StoryStatus.Ongoing, StoryStatus.Completed),
        (StoryStatus.Ongoing, StoryStatus.Hiatus),
        (StoryStatus.Hiatus, StoryStatus.Ongoing),
        (StoryStatus.Ongoing, StoryStatus.Dropped),
        (StoryStatus.Hiatus, StoryStatus.Dropped)
    };

    /// <summary>Whether a manual status change from <paramref name="from"/> to <paramref name="to"/> is allowed.</summary>
    public static bool CanTransitionManually(StoryStatus from, StoryStatus to)
    {
        return from != to && AllowedManualTransitions.Contains((from, to));
    }
}
