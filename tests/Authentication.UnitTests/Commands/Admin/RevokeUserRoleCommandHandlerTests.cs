using Authentication.Application.Commands.Admin.RevokeUserRole;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Commands.Admin;

public class RevokeUserRoleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly RevokeUserRoleCommandHandler _handler;

    public RevokeUserRoleCommandHandlerTests()
    {
        _handler = new RevokeUserRoleCommandHandler(
            _userRepository, Substitute.For<ILogger<RevokeUserRoleCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_RejectRevokingReader()
    {
        Func<Task> act = () => _handler.Handle(
            new RevokeUserRoleCommand { UserId = 1, Role = "Reader" }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        await _userRepository.DidNotReceive().RevokeRoleAsync(Arg.Any<long>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_UserMissing()
    {
        _userRepository.GetByIdAsync(9, Arg.Any<CancellationToken>()).Returns((User)null);

        Func<Task> act = () => _handler.Handle(
            new RevokeUserRoleCommand { UserId = 9, Role = "Moderator" }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_RevokeRoleAndReturnRemainingRoles()
    {
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new User { Id = 1 });
        _userRepository.GetRolesAsync(1, Arg.Any<CancellationToken>()).Returns(new List<Role> { Role.Reader });

        var result = await _handler.Handle(
            new RevokeUserRoleCommand { UserId = 1, Role = "Moderator" }, CancellationToken.None);

        await _userRepository.Received(1).RevokeRoleAsync(1, Role.Moderator, Arg.Any<CancellationToken>());
        result.Roles.Should().BeEquivalentTo("Reader");
    }
}
