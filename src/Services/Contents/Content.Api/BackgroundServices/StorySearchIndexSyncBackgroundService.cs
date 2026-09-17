using Content.Application.Commands.Stories.SyncStorySearchIndex;
using Content.Application.Constants;
using Content.Application.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Api.BackgroundServices;

/// <summary>
/// Timer loop that syncs stories/chapters changed since the last run into
/// Elasticsearch, by sending <see cref="SyncStorySearchIndexCommand"/> on each
/// tick. Deliberately mirrors <see cref="ScheduledChapterPublisher"/>: all the
/// actual work lives in the handler, this type only owns the schedule. Fully
/// decoupled from the write-path — no story/chapter command handler calls the
/// search service directly; this loop is the only writer to Elasticsearch.
/// Disabled via <c>StorySearchSync:Enabled=false</c>.
/// </summary>
public sealed class StorySearchIndexSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly StorySearchSyncOptions _options;
    private readonly ILogger<StorySearchIndexSyncBackgroundService> _logger;

    public StorySearchIndexSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<StorySearchSyncOptions> options,
        ILogger<StorySearchIndexSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs the timer loop for the lifetime of the host: on each tick, resolves a
    /// scoped <see cref="IMediator"/> and sends <see cref="SyncStorySearchIndexCommand"/>.
    /// No-op when disabled via config.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Feature flag: skip the whole loop when search sync is turned off.
        if (!_options.Enabled)
        {
            _logger.LogInformation(ApplicationLogConstants.StorySearchSyncDisabled);
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));
        _logger.LogInformation(ApplicationLogConstants.StorySearchSyncStarting, _options.PollIntervalSeconds);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                // Each tick gets its own DI scope so the mediator/handler use fresh,
                // short-lived scoped services (e.g. DbContext) rather than reusing state.
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new SyncStorySearchIndexCommand(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed pass must not kill the loop; the next tick retries.
                _logger.LogError(ex, ApplicationLogConstants.StorySearchSyncFailed);
            }
        }
        while (await SafeWaitAsync(timer, stoppingToken));
    }

    /// <summary>Waits for the next tick, swallowing cancellation so the caller's loop can exit cleanly.</summary>
    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
