namespace Be.StoryVerse.Cache.Constants;

/// <summary>
/// Redis constants shared across the cache project; feature-specific key names build on
/// <see cref="KeyPrefix"/> (see <see cref="RedisKeyConstants"/> for view tracking).
/// </summary>
public static class RedisConstants
{
    /// <summary>Namespaces every key so it can never collide with another application sharing the instance.</summary>
    public const string KeyPrefix = "storyverse:";

    /// <summary>
    /// Offset from UTC that defines the business day for view statistics (Vietnam, UTC+7).
    /// Fixed rather than a TimeZoneInfo lookup: Vietnam has no DST, and this avoids the
    /// Windows vs Linux time-zone id difference.
    /// </summary>
    public const int ViewStatsUtcOffsetHours = 7;
}
