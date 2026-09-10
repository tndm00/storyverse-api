using Authentication.Application.Commands.GoogleLogin;
using Authentication.Application.Constants;
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

namespace Authentication.UnitTests.Commands.GoogleLogin;

public class GoogleLoginCommandHandlerTests
{
    private readonly IGoogleTokenValidator _googleTokenValidator = Substitute.For<IGoogleTokenValidator>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserSessionIssuer _sessionIssuer = Substitute.For<IUserSessionIssuer>();
    private readonly ILogger<GoogleLoginCommandHandler> _logger =
        Substitute.For<ILogger<GoogleLoginCommandHandler>>();

    private readonly GoogleLoginCommandHandler _handler;

    public GoogleLoginCommandHandlerTests()
    {
        _handler = new GoogleLoginCommandHandler(_googleTokenValidator, _userRepository, _sessionIssuer, _logger);
    }

    private static GoogleLoginCommand CreateCommand()
    {
        return new GoogleLoginCommand { IdToken = "google-id-token" };
    }

    private static GoogleUserInfo CreateGoogleUser(
        string subject = "google-subject-1",
        string email = "reader@example.com",
        bool emailVerified = true,
        string name = "Google Reader",
        string pictureUrl = "https://example.com/avatar.png")
    {
        return new GoogleUserInfo(subject, email, emailVerified, name, pictureUrl);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_GoogleEmailIsNotVerified()
    {
        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>())
            .Returns(CreateGoogleUser(emailVerified: false));

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        (await act.Should().ThrowAsync<BadRequestException>()).Which.Message
            .Should().Be(ApplicationErrorConstants.GoogleAuthFailed);
    }

    [Fact]
    public async Task Handle_Should_ResolveByExternalSubjectId_When_UserAlreadyLinkedToGoogle()
    {
        var googleUser = CreateGoogleUser(subject: "google-subject-1");
        var existingUser = new User
        {
            Id = 7,
            Email = "reader@example.com",
            Status = UserStatus.Active,
            ExternalProvider = ApplicationConstants.GoogleProvider,
            ExternalId = "google-subject-1"
        };

        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>()).Returns(googleUser);
        _userRepository.GetByExternalIdAsync(ApplicationConstants.GoogleProvider, "google-subject-1", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var expectedSession = new LoginResponseDto { AccessToken = "access-token" };
        _sessionIssuer.IssueAsync(existingUser, Arg.Any<CancellationToken>()).Returns(expectedSession);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Should().BeSameAs(expectedSession);
        await _userRepository.DidNotReceive().GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_LinkGoogleToExistingAccount_When_NoSubjectMatchButEmailMatchesPasswordAccount()
    {
        var googleUser = CreateGoogleUser(subject: "google-subject-2", email: "reader@example.com");
        var existingUser = new User
        {
            Id = 9,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Active,
            AvatarUrl = null
        };

        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>()).Returns(googleUser);
        _userRepository.GetByExternalIdAsync(ApplicationConstants.GoogleProvider, "google-subject-2", Arg.Any<CancellationToken>())
            .Returns((User)null);
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(existingUser);

        var expectedSession = new LoginResponseDto { AccessToken = "access-token" };
        _sessionIssuer.IssueAsync(existingUser, Arg.Any<CancellationToken>()).Returns(expectedSession);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Should().BeSameAs(expectedSession);
        existingUser.ExternalProvider.Should().Be(ApplicationConstants.GoogleProvider);
        existingUser.ExternalId.Should().Be("google-subject-2");
        existingUser.AvatarUrl.Should().Be(googleUser.PictureUrl);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotOverwriteAvatar_When_LinkingAccountAlreadyHasOne()
    {
        var googleUser = CreateGoogleUser(subject: "google-subject-2", email: "reader@example.com");
        var existingUser = new User
        {
            Id = 9,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Active,
            AvatarUrl = "https://example.com/existing-avatar.png"
        };

        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>()).Returns(googleUser);
        _userRepository.GetByExternalIdAsync(ApplicationConstants.GoogleProvider, "google-subject-2", Arg.Any<CancellationToken>())
            .Returns((User)null);
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(existingUser);
        _sessionIssuer.IssueAsync(existingUser, Arg.Any<CancellationToken>())
            .Returns(new LoginResponseDto { AccessToken = "access-token" });

        await _handler.Handle(CreateCommand(), CancellationToken.None);

        existingUser.AvatarUrl.Should().Be("https://example.com/existing-avatar.png");
    }

    [Fact]
    public async Task Handle_Should_ProvisionNewReaderAccount_When_NoSubjectOrEmailMatchExists()
    {
        var googleUser = CreateGoogleUser(subject: "google-subject-3", email: "newreader@example.com");

        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>()).Returns(googleUser);
        _userRepository.GetByExternalIdAsync(ApplicationConstants.GoogleProvider, "google-subject-3", Arg.Any<CancellationToken>())
            .Returns((User)null);
        _userRepository.GetByEmailAsync("newreader@example.com", Arg.Any<CancellationToken>()).Returns((User)null);

        var expectedSession = new LoginResponseDto { AccessToken = "access-token" };
        _sessionIssuer.IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(expectedSession);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Should().BeSameAs(expectedSession);
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.Email == "newreader@example.com" &&
                u.PasswordHash == null &&
                u.ExternalProvider == ApplicationConstants.GoogleProvider &&
                u.ExternalId == "google-subject-3" &&
                u.Status == UserStatus.Active &&
                u.Roles.Any(r => r.Role == Role.Reader)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_ResolvedAccountIsNotActive()
    {
        var googleUser = CreateGoogleUser(subject: "google-subject-1");
        var existingUser = new User
        {
            Id = 7,
            Email = "reader@example.com",
            Status = UserStatus.Suspended,
            ExternalProvider = ApplicationConstants.GoogleProvider,
            ExternalId = "google-subject-1"
        };

        _googleTokenValidator.ValidateAsync("google-id-token", Arg.Any<CancellationToken>()).Returns(googleUser);
        _userRepository.GetByExternalIdAsync(ApplicationConstants.GoogleProvider, "google-subject-1", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        await _sessionIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
