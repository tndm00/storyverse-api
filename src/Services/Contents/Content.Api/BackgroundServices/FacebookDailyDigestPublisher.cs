using System;
using System.Threading;
using System.Threading.Tasks;
using Content.Application.Commands.Marketing.PostFacebookDigest;
using Content.Application.Constants;
using Content.Application.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Api.BackgroundServices;

/// <summary>
/// Timer loop that posts one Facebook "truyện hot hôm nay" digest per day, once
/// the UTC clock reaches <see cref="FacebookDigestOptions.PostHourUtc"/>. Ticks
/// every 30 minutes and tracks "already posted today" only in process memory
/// (no new DB table) — a container restart exactly at the post hour could post
/// twice that day, an accepted v1 tradeoff. Disabled via
/// <c>FacebookDigest:Enabled=false</c>.
/// </summary>
public sealed class FacebookDailyDigestPublisher : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FacebookDigestOptions _options;
    private readonly ILogger<FacebookDailyDigestPublisher> _logger;

    public FacebookDailyDigestPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<FacebookDigestOptions> options,
        ILogger<FacebookDailyDigestPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(ApplicationLogConstants.FacebookDigestPublisherDisabled);
            return;
        }

        _logger.LogInformation(ApplicationLogConstants.FacebookDigestPublisherStarting, _options.PostHourUtc);

        using var timer = new PeriodicTimer(PollInterval);
        var lastPostedDate = (DateOnly?)null;

        do
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = DateOnly.FromDateTime(now);

                if (now.Hour == _options.PostHourUtc && lastPostedDate != today)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(
                        new PostFacebookDigestCommand { TopCount = _options.TopCount }, stoppingToken);

                    lastPostedDate = today;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed pass must not kill the loop; the next tick retries.
                _logger.LogError(ex, ApplicationLogConstants.FacebookDigestFailed);
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
