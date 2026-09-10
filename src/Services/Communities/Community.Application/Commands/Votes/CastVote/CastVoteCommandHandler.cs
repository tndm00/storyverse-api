namespace Community.Application.Commands.Votes.CastVote;

public sealed class CastVoteCommandHandler : ICommandHandler<CastVoteCommand, CastVoteResultDto>
{
    private readonly IVoteRepository _voteRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<CastVoteCommandHandler> _logger;

    public CastVoteCommandHandler(
        IVoteRepository voteRepository,
        ICurrentUserContext userContext,
        ILogger<CastVoteCommandHandler> logger)
    {
        _voteRepository = voteRepository;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CastVoteResultDto> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var weekKey = IsoWeek.Current();

        var alreadyVoted = await _voteRepository.ExistsAsync(request.StoryId, userId, weekKey, cancellationToken);

        if (!alreadyVoted)
        {
            var vote = new Vote
            {
                StoryId = request.StoryId,
                UserId = userId,
                WeekKey = weekKey
            };

            await _voteRepository.AddAsync(vote, cancellationToken);
            await _voteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(ApplicationLogConstants.VoteCast, request.StoryId, weekKey, userId);
        }
        else
        {
            _logger.LogInformation(ApplicationLogConstants.VoteAlreadyCast, userId, request.StoryId, weekKey);
        }

        var weekCount = await _voteRepository.CountForStoryWeekAsync(request.StoryId, weekKey, cancellationToken);

        var now = DateTime.UtcNow;

        return new CastVoteResultDto
        {
            StoryId = request.StoryId,
            WeekKey = weekKey,
            Recorded = !alreadyVoted,
            WeekVoteCount = weekCount,
            PeriodStartUtc = IsoWeek.PeriodStartUtc(now),
            PeriodEndUtc = IsoWeek.PeriodEndUtc(now)
        };
    }
}
