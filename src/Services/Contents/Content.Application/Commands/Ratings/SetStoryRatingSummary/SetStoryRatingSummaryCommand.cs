namespace Content.Application.Commands.Ratings.SetStoryRatingSummary;

/// <summary>
/// Internal, service-to-service: applies the Community service's recomputed
/// rating aggregate (avg + count over all real ratings) to the denormalized
/// <see cref="Content.Domain.Entities.Story.RatingAvg"/> /
/// <see cref="Content.Domain.Entities.Story.RatingCount"/> fields. Not an author
/// action — reached only through the <c>X-Service-Token</c>-protected internal
/// endpoint, called best-effort by Community after a rating write commits.
/// </summary>
public sealed class SetStoryRatingSummaryCommand : ICommand<RatingSummaryResponseDto>
{
    public Guid StoryId { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }
}
