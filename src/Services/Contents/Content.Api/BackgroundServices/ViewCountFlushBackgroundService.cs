namespace Content.Api.BackgroundServices;

/// <summary>
/// Timer loop that moves view counts buffered in Redis into the Postgres <c>ViewCount</c> columns,
/// by sending <see cref="FlushViewCountsCommand"/> on each tick. Like
/// <see cref="StorySearchIndexSyncBackgroundService"/> it only owns the schedule; all work lives in
/// the handler. When Redis is disabled the handler finds nothing to take and the tick is a cheap no-op.
/// It also flushes once more on shutdown so a normal restart leaves nothing behind in Redis.
/// Disabled via <c>ViewCountFlush:Enabled=false</c>.
/// </summary>
public sealed class ViewCountFlushBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ViewCountFlushOptions _options;
    private readonly ILogger<ViewCountFlushBackgroundService> _logger;

    public ViewCountFlushBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ViewCountFlushOptions> options,
        ILogger<ViewCountFlushBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs the timer loop for the lifetime of the host: on each tick, flushes the buffered view
    /// counts. A failed pass is logged and retried on the next tick (the batch stays buffered).
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Feature flag: skip the whole loop when flushing is turned off.
        if (!_options.Enabled)
        {
            _logger.LogInformation(ApplicationLogConstants.ViewCountFlushDisabled);
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.FlushIntervalSeconds));
        _logger.LogInformation(ApplicationLogConstants.ViewCountFlushStarting, _options.FlushIntervalSeconds);

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await FlushOnceAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // A failed pass must not kill the loop; the batch is still buffered.
                    _logger.LogError(ex, ApplicationLogConstants.ViewCountFlushFailed);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Host is shutting down: leave the loop cleanly.
        }
    }

    /// <summary>Stops the loop, then performs one last flush so views buffered since the final tick are not left in Redis.</summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            await FlushOnceAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Nothing is lost: the parked batch stays in Redis and is served on the next start.
            _logger.LogError(ex, ApplicationLogConstants.ViewCountFinalFlushFailed);
        }
    }

    /// <summary>Sends one flush command in its own DI scope so it gets fresh scoped services (DbContext, repositories).</summary>
    private async Task FlushOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new FlushViewCountsCommand(), cancellationToken);
    }
}
