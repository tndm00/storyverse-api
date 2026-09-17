namespace Community.Application.Commands.Votes.CastVote;

/// <summary>Validation rules for <see cref="CastVoteCommand"/>.</summary>
public sealed class CastVoteCommandValidator : AbstractValidator<CastVoteCommand>
{
    /// <summary>Requires a non-empty story id.</summary>
    public CastVoteCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
