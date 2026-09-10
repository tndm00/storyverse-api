using Authentication.Application.Commands.Register;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ILogger<RegisterCommandHandler> _logger =
        Substitute.For<ILogger<RegisterCommandHandler>>();

    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepository, _passwordHasher, _logger);
    }

    private static RegisterCommand CreateCommand()
    {
        return new RegisterCommand
        {
            Email = "reader@example.com",
            Password = "Sup3rSecret!",
            DisplayName = "New Reader"
        };
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_EmailAlreadyRegistered()
    {
        _userRepository.ExistsByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CreateReaderAccount_When_EmailIsNotRegistered()
    {
        _userRepository.ExistsByEmailAsync("reader@example.com", Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash("Sup3rSecret!").Returns("hashed-password");

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Email.Should().Be("reader@example.com");
        result.DisplayName.Should().Be("New Reader");

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.Email == "reader@example.com" &&
                u.PasswordHash == "hashed-password" &&
                u.DisplayName == "New Reader" &&
                u.Roles.Any(r => r.Role == Role.Reader)),
            Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
