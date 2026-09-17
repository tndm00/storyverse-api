namespace Community.Application.Commands.Ratings.UpsertRating;

/// <summary>Validation rules for <see cref="UpsertRatingCommand"/>.</summary>
public sealed class UpsertRatingCommandValidator : AbstractValidator<UpsertRatingCommand>
{
    /// <summary>Requires a story id, a score within the allowed range, and an optional review text within the max length.</summary>
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
