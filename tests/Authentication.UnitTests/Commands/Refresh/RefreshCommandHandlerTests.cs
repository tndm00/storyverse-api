using Authentication.Application.Commands.Refresh;
using Authentication.Application.Dtos.Authentications.Sessions;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Commands.Refresh;

public class RefreshCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IUserSessionIssuer _sessionIssuer = Substitute.For<IUserSessionIssuer>();
    private readonly ILogger<RefreshCommandHandler> _logger = Substitute.For<ILogger<RefreshCommandHandler>>();

    private readonly RefreshCommandHandler _handler;

    public RefreshCommandHandlerTests()
    {
        _handler = new RefreshCommandHandler(
            _refreshTokenRepository, _userRepository, _tokenService, _sessionIssuer, _logger);

        // The handler hashes the incoming raw token; make the hash deterministic.
        _tokenService.HashRefreshToken("raw-token").Returns("HASH");
    }

    private static RefreshCommand Command() => new() { RefreshToken = "raw-token" };

    private static User ActiveUser(long id = 5) =>
        new() { Id = id, Email = "u@example.com", Status = UserStatus.Active };

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_When_TokenIsUnknown()
    {
        _refreshTokenRepository.GetByTokenHashAsync("HASH", Arg.Any<CancellationToken>()).Returns((RefreshToken)null);

        await _handler.Invoking(h => h.Handle(Command(), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_When_TokenIsExpired()
    {
        _refreshTokenRepository.GetByTokenHashAsync("HASH", Arg.Any<CancellationToken>())
            .Returns(new RefreshToken { UserId = 5, TokenHash = "HASH", ExpiresAt = DateTime.UtcNow.AddMinutes(-1) });

        await _handler.Invoking(h => h.Handle(Command(), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();

        await _sessionIssuer.DidNotReceive().IssueAsync(
            Arg.Any<User>(), Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RevokeWholeFamilyAndThrow_When_RevokedTokenIsReplayed()
    {
        _refreshTokenRepository.GetByTokenHashAsync("HASH", Arg.Any<CancellationToken>())
            .Returns(new RefreshToken
            {
                UserId = 5,
                TokenHash = "HASH",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                RevokedAt = DateTime.UtcNow.AddMinutes(-5)
            });

        await _handler.Invoking(h => h.Handle(Command(), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();

        await _refreshTokenRepository.Received(1).RevokeAllActiveForUserAsync(
            5, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_When_AccountIsNotActive()
    {
        _refreshTokenRepository.GetByTokenHashAsync("HASH", Arg.Any<CancellationToken>())
            .Returns(new RefreshToken { UserId = 5, TokenHash = "HASH", ExpiresAt = DateTime.UtcNow.AddDays(1) });
        _userRepository.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(new User { Id = 5, Status = UserStatus.Suspended });

        await _handler.Invoking(h => h.Handle(Command(), CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_Should_RotateAndReturnANewSession_When_TokenIsValid()
    {
        var stored = new RefreshToken
        {
            UserId = 5, TokenHash = "HASH", ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        _refreshTokenRepository.GetByTokenHashAsync("HASH", Arg.Any<CancellationToken>()).Returns(stored);
        var user = ActiveUser();
        _userRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(user);

        var newSession = new LoginResponseDto { AccessToken = "new-access", RefreshToken = "new-refresh" };
        _sessionIssuer.IssueAsync(user, stored, Arg.Any<CancellationToken>()).Returns(newSession);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Should().BeSameAs(newSession);
        await _sessionIssuer.Received(1).IssueAsync(user, stored, Arg.Any<CancellationToken>());
    }
}
