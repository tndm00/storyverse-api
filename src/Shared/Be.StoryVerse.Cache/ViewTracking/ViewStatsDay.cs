namespace Be.StoryVerse.Cache.ViewTracking;

/// <summary>
/// Defines the business day used by view statistics. A day is a Vietnam calendar day
/// (UTC+7, <see cref="RedisConstants.ViewStatsUtcOffsetHours"/>), so "today" and
/// "yesterday" match what the admin sees on their own clock rather than UTC boundaries.
/// </summary>
public static class ViewStatsDay
{
    /// <summary>Maps a UTC instant to the business day it falls on.</summary>
    /// <param name="utcNow">A UTC instant.</param>
    public static DateOnly Of(DateTime utcNow)
    {
        // Shift the instant into the business time zone, then keep only its calendar date.
        return DateOnly.FromDateTime(utcNow.AddHours(RedisConstants.ViewStatsUtcOffsetHours));
    }
}
