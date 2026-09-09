namespace Content.Application.Policies;

/// <summary>
/// Allowed <see cref="ChapterStatus"/> transitions, per product-workflow-context.md
/// section 7.
/// </summary>
public static class ChapterStatusPolicy
{
    public static bool CanPublish(ChapterStatus from)
    {
        return from is ChapterStatus.Draft or ChapterStatus.Scheduled;
    }

    public static bool CanSchedule(ChapterStatus from)
    {
        return from is ChapterStatus.Draft;
    }

    public static bool CanCancelSchedule(ChapterStatus from)
    {
        return from is ChapterStatus.Scheduled;
    }

    public static bool CanRemove(ChapterStatus from)
    {
        return from is ChapterStatus.Published;
    }
}
