namespace Content.Application.Queries.Stories.GetViewStats;

/// <summary>
/// Platform-wide view statistics for the admin (total, yesterday, today, top stories today).
/// Requires <c>analytics.view</c>, which only PlatformAdmin holds.
/// </summary>
public sealed class GetViewStatsQuery : IQuery<ViewStatsResponseDto>
{
}
