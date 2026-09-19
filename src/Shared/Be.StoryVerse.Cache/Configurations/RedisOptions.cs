namespace Be.StoryVerse.Cache.Configurations;

/// <summary>
/// Binds the <c>Redis</c> configuration section shared by every service. <see cref="Enabled"/> is the
/// kill switch: when false, Redis-backed features must fall back to their non-Redis behaviour and no
/// Redis connection is ever opened, so a feature can ship dark and be toggled with an env var.
/// </summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>Master switch for Redis-backed features.</summary>
    public bool Enabled { get; init; }

    /// <summary>StackExchange.Redis connection string, e.g. <c>redis:6379,password=...</c>.</summary>
    public string ConnectionString { get; init; } = "localhost:6379";

    /// <summary>
    /// Connect and per-command timeout. Kept short on purpose: Redis is often touched on a request's
    /// read path, so a dead Redis must fail fast and let the caller fall back instead of stalling.
    /// </summary>
    public int CommandTimeoutMs { get; init; } = 1000;
}
