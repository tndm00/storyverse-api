using System.IdentityModel.Tokens.Jwt;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Application.Options;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Infrastructure.Security;
using Be.StoryVerse.Shared.Constants;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Security;

public class UserSessionIssuerTests
{
    private readonly IAuthorProfileRepository _authorProfileRepository = Substitute.For<IAuthorProfileRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly UserSessionIssuer _issuer;

    public UserSessionIssuerTests()
    {
        var options = Substitute.For<IOptions<JwtOptions>>();
        options.Value.Returns(new JwtOptions
        {
            Issuer = "storyverse-tests",
            Audiences = new[] { "storyverse-web" },
            SecretKey = "this-is-a-test-signing-key-that-is-long-enough",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 14
        });

        _issuer = new UserSessionIssuer(
            new TokenService(options), _authorProfileRepository, _userRepository, _refreshTokenRepository);
    }

    private static User ActiveUser(long id = 7) =>
        new() { Id = id, Email = "author@example.com", DisplayName = "Author", Status = UserStatus.Active };

    [Fact]
    public async Task IssueAsync_Should_ReflectCurrentRolesAndAuthorId_ReadFromTheDatabase()
    {
        var user = ActiveUser();
        _userRepository.GetRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Role> { Role.Reader, Role.Author });
        _authorProfileRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new AuthorProfile { Id = 42, UserId = user.Id });

        var session = await _issuer.IssueAsync(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(session.AccessToken);
        jwt.Claims.Should().Contain(c => c.Type == AuthConstants.RolesClaimType && c.Value == nameof(Role.Author));
        jwt.Claims.Should().ContainSingle(c => c.Type == AuthConstants.AuthorProfileIdClaimType && c.Value == "42");
        session.RefreshToken.Should().NotBeNullOrWhiteSpace();

        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshToken>(t => t.UserId == user.Id && !string.IsNullOrEmpty(t.TokenHash)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueAsync_Should_RevokeAndLinkTheReplacedToken_OnRotation()
    {
        var user = ActiveUser();
        _userRepository.GetRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Role> { Role.Reader });

        var replaced = new RefreshToken { Id = 1, UserId = user.Id, TokenHash = "OLDHASH" };

        var session = await _issuer.IssueAsync(user, replaced);

        replaced.RevokedAt.Should().NotBeNull();
        replaced.ReplacedByTokenHash.Should().NotBeNullOrEmpty();
        replaced.ReplacedByTokenHash.Should().NotBe("OLDHASH");
        _refreshTokenRepository.Received(1).Update(replaced);
        session.AccessToken.Should().NotBeNullOrWhiteSpace();
    }
}
