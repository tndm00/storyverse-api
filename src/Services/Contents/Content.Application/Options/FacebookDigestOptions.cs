namespace Content.Application.Options;

/// <summary>
/// Binds the <c>FacebookDigest</c> configuration section: the background loop
/// that posts one "truyện hot hôm nay" digest to the Facebook Page per day.
/// </summary>
public sealed class FacebookDigestOptions
{
    public const string SectionName = "FacebookDigest";

    /// <summary>Master switch for the daily digest loop. Defaults to disabled until configured.</summary>
    public bool Enabled { get; init; }

    /// <summary>UTC hour (0-23) at which the digest is posted once per day.</summary>
    public int PostHourUtc { get; init; } = 2;

    /// <summary>How many top-viewed stories to list in the digest.</summary>
    public int TopCount { get; init; } = 5;
}
