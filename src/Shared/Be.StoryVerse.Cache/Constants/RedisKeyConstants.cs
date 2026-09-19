namespace Be.StoryVerse.Cache.Constants;

/// <summary>
/// Redis key names and formats for view tracking. Every key is namespaced with
/// <see cref="Prefix"/> so it can never collide with another application sharing the instance.
/// </summary>
public static class RedisKeyConstants
{
    public const string Prefix = RedisConstants.KeyPrefix;

    /// <summary>HASH storyId -&gt; views not yet added to Postgres (HINCRBY on every view).</summary>
    public const string PendingStoryViews = Prefix + "views:pending:story";

    /// <summary>HASH chapterId -&gt; views not yet added to Postgres.</summary>
    public const string PendingChapterViews = Prefix + "views:pending:chapter";

    /// <summary>
    /// The pending story hash after being RENAMEd out of the way by the flush job: a batch that is
    /// being (or was about to be) written to Postgres. Deleted only after the database commit.
    /// </summary>
    public const string FlushingStoryViews = Prefix + "views:flushing:story";

    /// <summary>The pending chapter hash after being RENAMEd out of the way by the flush job.</summary>
    public const string FlushingChapterViews = Prefix + "views:flushing:chapter";

    /// <summary>STRING with the first business day the daily counters were kept (SET NX).</summary>
    public const string TrackingSince = Prefix + "views:tracking-since";

    /// <summary>Per-day STRING counter; the day (<see cref="DayKeyFormat"/>) is appended.</summary>
    public const string DayViewsPrefix = Prefix + "views:day:";

    /// <summary>Per-day ZSET storyId -&gt; views (ZINCRBY); the day (<see cref="DayKeyFormat"/>) is appended.</summary>
    public const string TopStoriesPrefix = Prefix + "views:top:";

    /// <summary>Format of the day suffix in per-day keys.</summary>
    public const string DayKeyFormat = "yyyyMMdd";

    /// <summary>Format of the value stored under <see cref="TrackingSince"/>.</summary>
    public const string TrackingSinceFormat = "yyyy-MM-dd";

    /// <summary>Per-day keys expire this many days after their first write, so Redis never grows without bound.</summary>
    public const int DailyKeyRetentionDays = 35;
}
