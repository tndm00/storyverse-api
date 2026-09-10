namespace Community.Application.Text;

/// <summary>
/// Produces the ISO-8601 week key (<c>yyyy-Www</c>, e.g. <c>2026-W32</c>) used to
/// bucket weekly ranking votes. The ISO week-numbering year can differ from the
/// calendar year around January 1st, so both parts are derived together.
/// </summary>
public static class IsoWeek
{
    public static string Current() => From(DateTime.UtcNow);

    public static string From(DateTime instant)
    {
        var date = instant.Date;
        var week = System.Globalization.ISOWeek.GetWeekOfYear(date);
        var year = System.Globalization.ISOWeek.GetYear(date);

        return string.Create(CultureInfo.InvariantCulture, $"{year:D4}-W{week:D2}");
    }

    /// <summary>
    /// Start (inclusive) of the ISO week that contains <paramref name="instant"/>:
    /// the preceding Monday at 00:00 UTC. Votes reset on this boundary.
    /// </summary>
    public static DateTime PeriodStartUtc(DateTime instant)
    {
        var date = instant.Date;
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;

        return DateTime.SpecifyKind(date.AddDays(-daysSinceMonday), DateTimeKind.Utc);
    }

    /// <summary>
    /// End (exclusive) of the ISO week that contains <paramref name="instant"/>:
    /// the next Monday at 00:00 UTC. This is the next vote reset time.
    /// </summary>
    public static DateTime PeriodEndUtc(DateTime instant) => PeriodStartUtc(instant).AddDays(7);

    public static DateTime CurrentPeriodStartUtc() => PeriodStartUtc(DateTime.UtcNow);

    public static DateTime CurrentPeriodEndUtc() => PeriodEndUtc(DateTime.UtcNow);
}
