namespace Community.Application.Commands.Votes.CastVote;

public sealed class CastVoteCommandValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
