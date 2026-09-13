namespace Content.Application.Interfaces.Services;

/// <summary>
/// Posts to the configured Facebook Page's feed via the Graph API. Best-effort:
/// callers invoke this after their own work has committed and swallow failures.
/// A missing/disabled configuration short-circuits to a no-op.
/// </summary>
public interface IFacebookPageClient
{
    Task PostAsync(string message, string link, CancellationToken cancellationToken);
}
