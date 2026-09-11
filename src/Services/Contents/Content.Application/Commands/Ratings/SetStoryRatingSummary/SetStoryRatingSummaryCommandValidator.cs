namespace Content.Application.Commands.Ratings.SetStoryRatingSummary;

public sealed class SetStoryRatingSummaryCommandValidator : AbstractValidator<SetStoryRatingSummaryCommand>
{
    public SetStoryRatingSummaryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
        RuleFor(x => x.RatingAvg).InclusiveBetween(0m, 5m);
        RuleFor(x => x.RatingCount).GreaterThanOrEqualTo(0);
    }
}
