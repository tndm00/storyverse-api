namespace Content.Application.Policies;

/// <summary>
/// Allowed <see cref="ChapterStatus"/> transitions. A chapter is never
/// authored straight to <see cref="ChapterStatus.Published"/> — the author
/// submits it for review, a moderator picks it up, then approves or rejects.
/// </summary>
public static class ChapterStatusPolicy
{
    /// <summary>Author submits (or resubmits after rejection) for moderation.</summary>
    public static bool CanSubmitForReview(ChapterStatus from)
    {
        return from is ChapterStatus.Draft or ChapterStatus.Rejected;
    }

    /// <summary>Moderator picks the item up off the queue.</summary>
    public static bool CanStartReview(ChapterStatus from)
    {
        return from is ChapterStatus.PendingReview;
    }

    /// <summary>Moderator approves — the chapter is published.</summary>
    public static bool CanApprove(ChapterStatus from)
    {
        return from is ChapterStatus.InReview;
    }

    /// <summary>Moderator rejects — the author may edit and resubmit.</summary>
    public static bool CanReject(ChapterStatus from)
    {
        return from is ChapterStatus.InReview;
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
