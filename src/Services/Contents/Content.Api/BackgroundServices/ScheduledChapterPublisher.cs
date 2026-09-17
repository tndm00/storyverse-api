using System;
using System.Threading;
using System.Threading.Tasks;
using Content.Application.Commands.Chapters.PublishDueChapters;
using Content.Application.Constants;
using Content.Application.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Api.BackgroundServices;

/// <summary>
/// Timer loop that publishes <c>Scheduled</c> chapters once their time arrives,
/// by sending <see cref="PublishDueChaptersCommand"/> on each tick. All the work
/// (and its concurrency guarantees) lives in the handler; this type only owns
/// the schedule and turns config on/off. Disabled via
/// <c>ChapterPublishing:Enabled=false</c>.
/// </summary>
public sealed class ScheduledChapterPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChapterPublishingOptions _options;
    private readonly ILogger<ScheduledChapterPublisher> _logger;

    public ScheduledChapterPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<ChapterPublishingOptions> options,
        ILogger<ScheduledChapterPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Runs the timer loop for the lifetime of the host: on each tick, resolves a
    /// scoped <see cref="IMediator"/> and sends <see cref="PublishDueChaptersCommand"/>.
    /// No-op when disabled via config.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Feature flag: skip the whole loop when publishing is turned off.
        if (!_options.Enabled)
        {
            _logger.LogInformation(ApplicationLogConstants.ScheduledChapterPublisherDisabled);
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, _options.PollIntervalSeconds));
        _logger.LogInformation(
            ApplicationLogConstants.ScheduledChapterPublisherStarting, _options.PollIntervalSeconds);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                // Each tick gets its own DI scope so the mediator/handler use fresh,
                // short-lived scoped services (e.g. DbContext) rather than reusing state.
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(
                    new PublishDueChaptersCommand { BatchSize = _options.BatchSize }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed pass must not kill the loop; the next tick retries.
                _logger.LogError(ex, ApplicationLogConstants.ScheduledChapterPublisherFailed);
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
