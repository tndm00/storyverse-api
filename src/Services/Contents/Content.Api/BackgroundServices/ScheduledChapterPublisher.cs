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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
