using Be.StoryVerse.Cache.ViewTracking;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Policies;

public class ViewStatsDayTests
{
    [Fact]
    public void Of_Should_ReturnSameCalendarDay_When_UtcInstantIsMidday()
    {
        var day = ViewStatsDay.Of(new DateTime(2026, 9, 18, 5, 0, 0, DateTimeKind.Utc));

        day.Should().Be(new DateOnly(2026, 9, 18));
    }

    [Fact]
    public void Of_Should_RollToNextDay_When_UtcInstantIsAfter1700()
    {
        // 17:30 UTC is 00:30 the next day in Vietnam (UTC+7).
        var day = ViewStatsDay.Of(new DateTime(2026, 9, 18, 17, 30, 0, DateTimeKind.Utc));

        day.Should().Be(new DateOnly(2026, 9, 19));
    }

    [Fact]
    public void Of_Should_StayOnSameDay_When_UtcInstantIsJustBefore1700()
    {
        var day = ViewStatsDay.Of(new DateTime(2026, 9, 18, 16, 59, 59, DateTimeKind.Utc));

        day.Should().Be(new DateOnly(2026, 9, 18));
    }

    [Fact]
    public void Of_Should_CrossMonthBoundary_When_LastDayOfMonthRollsOver()
    {
        var day = ViewStatsDay.Of(new DateTime(2026, 9, 30, 18, 0, 0, DateTimeKind.Utc));

        day.Should().Be(new DateOnly(2026, 10, 1));
    }
}
