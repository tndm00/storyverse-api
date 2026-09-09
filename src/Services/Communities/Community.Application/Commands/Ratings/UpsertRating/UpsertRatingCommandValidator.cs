namespace Community.Application.Commands.Ratings.UpsertRating;

public sealed class UpsertRatingCommandValidator : AbstractValidator<UpsertRatingCommand>
{
    public UpsertRatingCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Score)
            .InclusiveBetween(ApplicationConstants.MinRatingScore, ApplicationConstants.MaxRatingScore)
            .WithMessage(ApplicationErrorConstants.InvalidRatingScore);

        RuleFor(x => x.ReviewText)
            .MaximumLength(ApplicationConstants.MaxReviewTextLength)
            .WithMessage(ApplicationErrorConstants.ReviewTextTooLong)
            .When(x => x.ReviewText is not null);
    }
}
