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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
