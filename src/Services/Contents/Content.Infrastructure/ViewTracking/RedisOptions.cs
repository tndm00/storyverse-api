namespace Content.Infrastructure.ViewTracking;

/// <summary>
/// Binds the <c>Redis</c> configuration section. <see cref="Enabled"/> is the kill switch:
/// false makes <see cref="RedisViewStore"/> fall back to direct Postgres increments and never
/// opens a Redis connection, so the feature can ship dark and be toggled with an env var.
/// </summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>Master switch for Redis-backed view tracking.</summary>
    public bool Enabled { get; init; }

    /// <summary>StackExchange.Redis connection string, e.g. <c>redis:6379,password=...</c>.</summary>
    public string ConnectionString { get; init; } = "localhost:6379";

    /// <summary>
    /// Connect and per-command timeout. Kept short on purpose: a view is recorded on the read path,
    /// so a dead Redis must fail fast and fall back instead of stalling page loads.
    /// </summary>
    public int CommandTimeoutMs { get; init; } = 1000;
}
