namespace Community.Application.Queries.Votes.GetWeekVoteCount;

/// <summary>Handles <see cref="GetWeekVoteCountQuery"/>: returns the current ISO week's vote count for a story.</summary>
public sealed class GetWeekVoteCountQueryHandler : IQueryHandler<GetWeekVoteCountQuery, VoteCountResponseDto>
{
    private readonly IVoteRepository _voteRepository;

    /// <summary>Creates the handler with its vote repository dependency.</summary>
    public GetWeekVoteCountQueryHandler(IVoteRepository voteRepository)
    {
        _voteRepository = voteRepository;
    }

    /// <summary>Returns the story's vote count for the current ISO week along with the period boundaries.</summary>
    public async Task<VoteCountResponseDto> Handle(GetWeekVoteCountQuery request, CancellationToken cancellationToken)
    {
        // Resolve the current ISO week key and count votes for the story within it.
        var weekKey = IsoWeek.Current();
        var count = await _voteRepository.CountForStoryWeekAsync(request.StoryId, weekKey, cancellationToken);

        var now = DateTime.UtcNow;

        // Attach the week's start/end boundaries so the client can render a countdown.
        return new VoteCountResponseDto
        {
            StoryId = request.StoryId,
            WeekKey = weekKey,
            WeekVoteCount = count,
            PeriodStartUtc = IsoWeek.PeriodStartUtc(now),
            PeriodEndUtc = IsoWeek.PeriodEndUtc(now)
        };
    }
}
