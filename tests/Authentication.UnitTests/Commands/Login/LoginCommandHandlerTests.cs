using Authentication.Application.Commands.Login;
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

namespace Authentication.UnitTests.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUserSessionIssuer _sessionIssuer = Substitute.For<IUserSessionIssuer>();
    private readonly ILogger<LoginCommandHandler> _logger =
        Substitute.For<ILogger<LoginCommandHandler>>();

    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _sessionIssuer, _logger);
    }

    private static LoginCommand CreateCommand()
    {
        return new LoginCommand { Email = "reader@example.com", Password = "Sup3rSecret!" };
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestExceptionWithInvalidCredentials_When_NoUserExistsForEmail()
    {
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns((User)null);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        (await act.Should().ThrowAsync<BadRequestException>()).Which.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestExceptionWithInvalidCredentials_When_PasswordIsWrong()
    {
        var user = new User
        {
            Id = 1,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Active
        };
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Sup3rSecret!", "hashed-password").Returns(false);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        (await act.Should().ThrowAsync<BadRequestException>()).Which.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_Should_ThrowSameExceptionTypeAndMessage_ForUnknownEmailAndWrongPassword()
    {
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns((User)null);
        Func<Task> unknownEmailAct = () => _handler.Handle(CreateCommand(), CancellationToken.None);
        var unknownEmailException = await unknownEmailAct.Should().ThrowAsync<BadRequestException>();

        var user = new User
        {
            Id = 1,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Active
        };
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Sup3rSecret!", "hashed-password").Returns(false);
        Func<Task> wrongPasswordAct = () => _handler.Handle(CreateCommand(), CancellationToken.None);
        var wrongPasswordException = await wrongPasswordAct.Should().ThrowAsync<BadRequestException>();

        wrongPasswordException.Which.Message.Should().Be(unknownEmailException.Which.Message);
        wrongPasswordException.Which.GetType().Should().Be(unknownEmailException.Which.GetType());
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_When_AccountIsNotActive()
    {
        var user = new User
        {
            Id = 1,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Suspended
        };
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Sup3rSecret!", "hashed-password").Returns(true);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        (await act.Should().ThrowAsync<BadRequestException>()).Which.Message.Should().Be("Invalid email or password.");
        await _sessionIssuer.DidNotReceive().IssueAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_IssueSession_When_CredentialsAreValidAndAccountIsActive()
    {
        var user = new User
        {
            Id = 1,
            Email = "reader@example.com",
            PasswordHash = "hashed-password",
            Status = UserStatus.Active
        };
        _userRepository.GetByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("Sup3rSecret!", "hashed-password").Returns(true);

        var expectedSession = new LoginResponseDto { AccessToken = "access-token", RefreshToken = "refresh-token" };
        _sessionIssuer.IssueAsync(user, Arg.Any<CancellationToken>()).Returns(expectedSession);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Should().BeSameAs(expectedSession);
        await _sessionIssuer.Received(1).IssueAsync(user, Arg.Any<CancellationToken>());
    }
}
