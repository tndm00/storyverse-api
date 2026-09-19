namespace Be.StoryVerse.Cache.ViewTracking;

/// <summary>
/// Redis-backed view tracking. One class implements the three views of the same data:
/// <list type="bullet">
/// <item><see cref="IViewTracker"/> — the hot path, one atomic MULTI/EXEC per page view;</item>
/// <item><see cref="IViewCountBuffer"/> — the hand-off to the periodic Postgres flush;</item>
/// <item><see cref="IViewStatsReader"/> — the admin's daily totals and top stories.</item>
/// </list>
/// Recording never throws: if Redis is disabled or misbehaves the view is counted with a direct
/// database increment (<see cref="IViewCountFallback"/>) instead, so no view is lost and reading a chapter never fails because of Redis.
/// </summary>
public sealed class RedisViewStore : IViewTracker, IViewCountBuffer, IViewStatsReader
{
    private readonly Lazy<IConnectionMultiplexer> _redis;
    private readonly RedisOptions _options;
    private readonly IViewCountFallback _fallback;
    private readonly ILogger<RedisViewStore> _logger;

    public RedisViewStore(
        Lazy<IConnectionMultiplexer> redis,
        IOptions<RedisOptions> options,
        IViewCountFallback fallback,
        ILogger<RedisViewStore> logger)
    {
        _redis = redis;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    /// <summary>Counts a story-page view in Redis, or through the fallback (direct database increment) when Redis is off/failing.</summary>
    public async Task RecordStoryViewAsync(long storyId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            try
            {
                await BufferViewAsync(storyId, chapterId: null, cancellationToken);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Redis trouble must never break reading: count the view through the fallback instead.
                _logger.LogWarning(ex, CacheLogConstants.ViewRecordFallback, "story", storyId);
            }
        }

        await _fallback.IncrementStoryAsync(storyId, cancellationToken);
    }

    /// <summary>Counts a chapter read (and its parent story's view) in Redis, or through the fallback (direct database increment) when Redis is off/failing.</summary>
    public async Task RecordChapterViewAsync(long chapterId, long storyId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            try
            {
                await BufferViewAsync(storyId, chapterId, cancellationToken);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, CacheLogConstants.ViewRecordFallback, "chapter", chapterId);
            }
        }

        await _fallback.IncrementChapterAsync(chapterId, storyId, cancellationToken);
    }

    /// <summary>
    /// Buffers one view with a single MULTI/EXEC, so either every counter moves or none does (which
    /// also makes the Postgres fallback safe: a failed transaction never half-counts a view).
    /// </summary>
    private async Task BufferViewAsync(long storyId, long? chapterId, CancellationToken cancellationToken)
    {
        var today = ViewStatsDay.Of(DateTime.UtcNow);
        var dayKey = DayViewsKey(today);
        var topKey = TopStoriesKey(today);
        var retention = TimeSpan.FromDays(RedisKeyConstants.DailyKeyRetentionDays);

        var transaction = _redis.Value.GetDatabase().CreateTransaction();

        var commands = new List<Task>
        {
            // Waiting-to-flush counters: added to the Postgres ViewCount columns by the flush job.
            transaction.HashIncrementAsync(RedisKeyConstants.PendingStoryViews, storyId),

            // Today's total, and today's per-story ranking used for the "top stories" list.
            transaction.StringIncrementAsync(dayKey),
            transaction.SortedSetIncrementAsync(topKey, storyId, 1),

            // Give per-day keys a TTL once (HasNoExpiry = only if they do not have one yet).
            transaction.KeyExpireAsync(dayKey, retention, ExpireWhen.HasNoExpiry),
            transaction.KeyExpireAsync(topKey, retention, ExpireWhen.HasNoExpiry),

            // Remember the first day we ever counted, so the UI can explain why older days are zero.
            transaction.StringSetAsync(
                RedisKeyConstants.TrackingSince,
                today.ToString(RedisKeyConstants.TrackingSinceFormat, CultureInfo.InvariantCulture),
                when: When.NotExists)
        };

        if (chapterId is { } id)
        {
            commands.Add(transaction.HashIncrementAsync(RedisKeyConstants.PendingChapterViews, id));
        }

        // EXEC applies all queued commands atomically; then observe each command's result.
        await transaction.ExecuteAsync();
        await Task.WhenAll(commands).WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Moves the pending hashes out of the way with RENAME (atomic: views arriving after this instant
    /// start a fresh hash) and reads them. A batch that was taken but never committed is served again.
    /// </summary>
    public async Task<PendingViewCounts> TakePendingAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new PendingViewCounts();
        }

        var db = _redis.Value.GetDatabase();

        await ParkAsync(db, RedisKeyConstants.PendingStoryViews, RedisKeyConstants.FlushingStoryViews);
        await ParkAsync(db, RedisKeyConstants.PendingChapterViews, RedisKeyConstants.FlushingChapterViews);

        return new PendingViewCounts
        {
            Stories = await ReadCountsAsync(db, RedisKeyConstants.FlushingStoryViews),
            Chapters = await ReadCountsAsync(db, RedisKeyConstants.FlushingChapterViews)
        };
    }

    /// <summary>Deletes the parked batch once Postgres has it. Until then it survives a crash and is re-served.</summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await _redis.Value.GetDatabase().KeyDeleteAsync(new RedisKey[]
        {
            RedisKeyConstants.FlushingStoryViews,
            RedisKeyConstants.FlushingChapterViews
        });
    }

    /// <summary>Reads today's/yesterday's totals, the unflushed views and today's top stories in one round trip.</summary>
    public async Task<ViewStatsSnapshot> GetSnapshotAsync(int topCount, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new ViewStatsSnapshot { IsAvailable = false };
        }

        try
        {
            var db = _redis.Value.GetDatabase();
            var today = ViewStatsDay.Of(DateTime.UtcNow);

            // Issue every read up front (pipelined), then await them together.
            var todayViews = db.StringGetAsync(DayViewsKey(today));
            var yesterdayViews = db.StringGetAsync(DayViewsKey(today.AddDays(-1)));
            var pending = db.HashValuesAsync(RedisKeyConstants.PendingStoryViews);
            var flushing = db.HashValuesAsync(RedisKeyConstants.FlushingStoryViews);
            var top = db.SortedSetRangeByRankWithScoresAsync(TopStoriesKey(today), 0, topCount - 1, Order.Descending);
            var trackingSince = db.StringGetAsync(RedisKeyConstants.TrackingSince);

            await Task.WhenAll(todayViews, yesterdayViews, pending, flushing, top, trackingSince)
                .WaitAsync(cancellationToken);

            return new ViewStatsSnapshot
            {
                IsAvailable = true,
                TodayViews = ToLong(todayViews.Result),
                YesterdayViews = ToLong(yesterdayViews.Result),
                // Not yet in Postgres: still pending, or parked by a flush that has not committed.
                PendingStoryViews = pending.Result.Sum(ToLong) + flushing.Result.Sum(ToLong),
                TrackingSince = ParseTrackingSince(trackingSince.Result),
                TopStoriesToday = top.Result
                    .Select(entry => new TopStoryViews { StoryId = ToLong(entry.Element), Views = (long)entry.Score })
                    .ToList()
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, CacheLogConstants.ViewStatsReadFailed);
            return new ViewStatsSnapshot { IsAvailable = false };
        }
    }

    /// <summary>Renames <paramref name="pendingKey"/> to <paramref name="flushingKey"/> unless a batch is already parked there.</summary>
    private static async Task ParkAsync(IDatabase db, string pendingKey, string flushingKey)
    {
        // A leftover parked batch was never committed to Postgres: serve it first.
        if (await db.KeyExistsAsync(flushingKey))
        {
            return;
        }

        // RENAME errors on a missing source key, so only rename when there is something to move.
        if (await db.KeyExistsAsync(pendingKey))
        {
            await db.KeyRenameAsync(pendingKey, flushingKey);
        }
    }

    /// <summary>Reads a HASH of id -&gt; count into a dictionary.</summary>
    private static async Task<IReadOnlyDictionary<long, long>> ReadCountsAsync(IDatabase db, string key)
    {
        var entries = await db.HashGetAllAsync(key);

        return entries.ToDictionary(entry => ToLong(entry.Name), entry => ToLong(entry.Value));
    }

    private static string DayViewsKey(DateOnly day)
    {
        return RedisKeyConstants.DayViewsPrefix + day.ToString(RedisKeyConstants.DayKeyFormat, CultureInfo.InvariantCulture);
    }

    private static string TopStoriesKey(DateOnly day)
    {
        return RedisKeyConstants.TopStoriesPrefix + day.ToString(RedisKeyConstants.DayKeyFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>Redis returns a missing key as a null value; treat it as zero.</summary>
    private static long ToLong(RedisValue value)
    {
        return value.HasValue ? (long)value : 0;
    }

    private static DateOnly? ParseTrackingSince(RedisValue value)
    {
        return value.HasValue
            && DateOnly.TryParseExact(
                (string)value, RedisKeyConstants.TrackingSinceFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? day
            : null;
    }
}
