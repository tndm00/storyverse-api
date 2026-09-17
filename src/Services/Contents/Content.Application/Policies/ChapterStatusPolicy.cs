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

    /// <summary>Moderator picks the item up off the queue. A previously rejected
    /// chapter can also be sent back into review to go through the normal
    /// approve/reject flow again.</summary>
    public static bool CanStartReview(ChapterStatus from)
    {
        return from is ChapterStatus.PendingReview or ChapterStatus.Rejected;
    }

    /// <summary>Moderator approves — the chapter is published. Only reachable from
    /// <see cref="ChapterStatus.InReview"/>; a rejected chapter must go back through
    /// review first.</summary>
    public static bool CanApprove(ChapterStatus from)
    {
        return from is ChapterStatus.InReview;
    }

    /// <summary>Moderator rejects — the author may edit and resubmit.</summary>
    public static bool CanReject(ChapterStatus from)
    {
        return from is ChapterStatus.InReview;
    }

    /// <summary>Author schedules a draft chapter to auto-publish at a future time.</summary>
    public static bool CanSchedule(ChapterStatus from)
    {
        return from is ChapterStatus.Draft;
    }

    /// <summary>Author cancels a pending schedule, returning the chapter to Draft.</summary>
    public static bool CanCancelSchedule(ChapterStatus from)
    {
        return from is ChapterStatus.Scheduled;
    }

    /// <summary>Author removes a chapter that is already Published.</summary>
    public static bool CanRemove(ChapterStatus from)
    {
        return from is ChapterStatus.Published;
    }
}
