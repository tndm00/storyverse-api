namespace Community.Application.Queries.Votes.GetWeekVoteCount;

/// <summary>Validation rules for <see cref="GetWeekVoteCountQuery"/>.</summary>
public sealed class GetWeekVoteCountQueryValidator : AbstractValidator<GetWeekVoteCountQuery>
{
    /// <summary>Requires a non-empty story id.</summary>
    public GetWeekVoteCountQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
