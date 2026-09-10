using Community.Application.Queries.Votes.GetCurrentVotePeriod;
using Community.Application.Text;
using FluentAssertions;
using Xunit;

namespace Community.UnitTests.Queries.Votes;

public class GetCurrentVotePeriodQueryHandlerTests
{
    private readonly GetCurrentVotePeriodQueryHandler _handler = new();

    [Fact]
    public async Task Handle_Should_ReturnWeekBoundaries_That_AreMondayAlignedAndOneWeekApart()
    {
        var result = await _handler.Handle(new GetCurrentVotePeriodQuery(), CancellationToken.None);

        result.PeriodStartUtc.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.PeriodStartUtc.TimeOfDay.Should().Be(TimeSpan.Zero);
        result.PeriodEndUtc.Should().Be(result.PeriodStartUtc.AddDays(7));
        result.WeekKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnPeriod_That_ContainsNow()
    {
        var result = await _handler.Handle(new GetCurrentVotePeriodQuery(), CancellationToken.None);

        var now = DateTime.UtcNow;
        result.PeriodStartUtc.Should().BeOnOrBefore(now);
        result.PeriodEndUtc.Should().BeAfter(now);
    }

    [Theory]
    [InlineData("2026-09-10", "2026-09-07", "2026-09-14")] // Thursday -> Mon 7th .. Mon 14th
    [InlineData("2026-09-07", "2026-09-07", "2026-09-14")] // Monday itself is the start
    [InlineData("2026-09-13", "2026-09-07", "2026-09-14")] // Sunday still in the same week
    public void IsoWeek_PeriodBoundaries_AreComputedFromMonday(string instant, string expectedStart, string expectedEnd)
    {
        var date = DateTime.Parse(instant);

        IsoWeek.PeriodStartUtc(date).Should().Be(DateTime.Parse(expectedStart));
        IsoWeek.PeriodEndUtc(date).Should().Be(DateTime.Parse(expectedEnd));
        IsoWeek.PeriodStartUtc(date).Kind.Should().Be(DateTimeKind.Utc);
    }
}
