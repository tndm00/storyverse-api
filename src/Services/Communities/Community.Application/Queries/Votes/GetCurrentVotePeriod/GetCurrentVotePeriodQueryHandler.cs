namespace Community.Application.Queries.Votes.GetCurrentVotePeriod;

/// <summary>Handles <see cref="GetCurrentVotePeriodQuery"/>: computes the current ISO week's voting period boundaries.</summary>
public sealed class GetCurrentVotePeriodQueryHandler
    : IQueryHandler<GetCurrentVotePeriodQuery, VotePeriodResponseDto>
{
    /// <summary>Returns the current ISO week key and its UTC start/end boundaries.</summary>
    public Task<VotePeriodResponseDto> Handle(
        GetCurrentVotePeriodQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Derive the current ISO week key and its start/end boundaries from "now".
        return Task.FromResult(new VotePeriodResponseDto
        {
            WeekKey = IsoWeek.From(now),
            PeriodStartUtc = IsoWeek.PeriodStartUtc(now),
            PeriodEndUtc = IsoWeek.PeriodEndUtc(now)
        });
    }
}
