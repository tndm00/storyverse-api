namespace Community.Application.Commands.Votes.CastVote;

/// <summary>
/// Casts the caller's weekly ranking vote for a story in the current ISO week.
/// Idempotent: a second call in the same week is a no-op that still returns 200.
/// </summary>
public sealed class CastVoteCommand : ICommand<CastVoteResultDto>
{
    public Guid StoryId { get; init; }
}
