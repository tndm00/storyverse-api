using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Authentication.Api.Seed;

/// <summary>
/// Dev-time bootstrap for platform administrators. On startup, promotes every
/// account listed under <c>Seed:PlatformAdminEmails</c> to
/// <see cref="Role.PlatformAdmin"/> once that account has registered.
/// <para>
/// Idempotent and safe to leave enabled: it is a no-op when the list is empty or
/// a listed account does not exist yet. Real environments leave the list empty
/// and grant the role through an admin tool.
/// </para>
/// </summary>
public sealed class PlatformAdminSeeder : IHostedService
{
    private const string ConfigKey = "Seed:PlatformAdminEmails";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PlatformAdminSeeder> _logger;

    public PlatformAdminSeeder(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<PlatformAdminSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var emails = _configuration.GetSection(ConfigKey).Get<string[]>() ?? Array.Empty<string>();
        if (emails.Length == 0)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        foreach (var email in emails)
        {
            try
            {
                var user = await users.GetByEmailAsync(email, cancellationToken);
                if (user is null)
                {
                    _logger.LogInformation(
                        "Platform-admin seed: no account for {Email} yet; will retry on next startup.", email);
                    continue;
                }

                var roles = await users.GetRolesAsync(user.Id, cancellationToken);
                if (roles.Contains(Role.PlatformAdmin))
                {
                    continue;
                }

                await users.GrantRoleAsync(user.Id, Role.PlatformAdmin, cancellationToken);
                await users.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Platform-admin seed: granted PlatformAdmin to user {UserId} ({Email}).", user.Id, email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Platform-admin seed failed for {Email}.", email);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
