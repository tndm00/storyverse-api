namespace Community.Application.Queries.Votes.GetCurrentVotePeriod;

public sealed class GetCurrentVotePeriodQueryHandler
    : IQueryHandler<GetCurrentVotePeriodQuery, VotePeriodResponseDto>
{
    public Task<VotePeriodResponseDto> Handle(
        GetCurrentVotePeriodQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return Task.FromResult(new VotePeriodResponseDto
        {
            WeekKey = IsoWeek.From(now),
            PeriodStartUtc = IsoWeek.PeriodStartUtc(now),
            PeriodEndUtc = IsoWeek.PeriodEndUtc(now)
        });
    }
}
