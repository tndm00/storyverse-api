namespace Community.Application.Queries.Votes.GetWeekVoteCount;

public sealed class GetWeekVoteCountQueryValidator : AbstractValidator<GetWeekVoteCountQuery>
{
    public GetWeekVoteCountQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
