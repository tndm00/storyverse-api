using System.IdentityModel.Tokens.Jwt;
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

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
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

        _tokenService = new TokenService(options);
    }

    private static User CreateUser(long id = 1)
    {
        return new User { Id = id, Email = "user@example.com", DisplayName = "Test User" };
    }

    private static JwtSecurityToken ReadToken(string value)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(value);
    }

    [Fact]
    public void GenerateAccessToken_Should_NotIncludeAuthorIdClaim_When_UserHasNoAuthorProfile()
    {
        var token = _tokenService.GenerateAccessToken(CreateUser(), authorProfileId: null, roles: new[] { Role.Reader });

        var jwt = ReadToken(token.Value);

        jwt.Claims.Should().NotContain(c => c.Type == AuthConstants.AuthorProfileIdClaimType);
    }

    [Fact]
    public void GenerateAccessToken_Should_NotIncludeAuthorIdClaim_When_AuthorProfileIdIsZero()
    {
        // 0 means "no author" (GuestPublish's shared guest author id) - never a real claim value.
        var token = _tokenService.GenerateAccessToken(CreateUser(), authorProfileId: 0, roles: new[] { Role.Reader });

        var jwt = ReadToken(token.Value);

        jwt.Claims.Should().NotContain(c => c.Type == AuthConstants.AuthorProfileIdClaimType);
    }

    [Fact]
    public void GenerateAccessToken_Should_IncludeAuthorIdClaimAndAuthorRole_When_UserHasAuthorProfile()
    {
        var token = _tokenService.GenerateAccessToken(CreateUser(), authorProfileId: 42, roles: new[] { Role.Reader, Role.Author });

        var jwt = ReadToken(token.Value);

        jwt.Claims.Should().ContainSingle(c => c.Type == AuthConstants.AuthorProfileIdClaimType && c.Value == "42");
        jwt.Claims.Should().Contain(c => c.Type == AuthConstants.RolesClaimType && c.Value == Role.Author.ToString());
    }
}
