namespace Community.Application.Interfaces.Services;

/// <summary>
/// Pushes the recomputed rating aggregate (avg + count over all real ratings)
/// for a story to the Content service, which stores it denormalized on
/// <c>Story.RatingAvg</c> / <c>Story.RatingCount</c> for story listings. Called
/// best-effort after a rating write commits. Best-effort by contract: a sync
/// failure is logged and swallowed, never thrown, so it never fails the
/// caller's rating write.
/// </summary>
public interface IContentRatingSyncClient
{
    Task SyncRatingSummaryAsync(
        Guid storyId, decimal ratingAvg, int ratingCount, CancellationToken cancellationToken);
}
