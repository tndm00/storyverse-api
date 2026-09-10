namespace Community.Application.Queries.Votes.GetWeekVoteCount;

public sealed class GetWeekVoteCountQueryHandler : IQueryHandler<GetWeekVoteCountQuery, VoteCountResponseDto>
{
    private readonly IVoteRepository _voteRepository;

    public GetWeekVoteCountQueryHandler(IVoteRepository voteRepository)
    {
        _voteRepository = voteRepository;
    }

    public async Task<VoteCountResponseDto> Handle(GetWeekVoteCountQuery request, CancellationToken cancellationToken)
    {
        var weekKey = IsoWeek.Current();
        var count = await _voteRepository.CountForStoryWeekAsync(request.StoryId, weekKey, cancellationToken);

        var now = DateTime.UtcNow;

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
