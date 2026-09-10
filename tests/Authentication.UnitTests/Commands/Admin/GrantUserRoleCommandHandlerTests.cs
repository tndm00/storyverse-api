using Authentication.Application.Commands.Admin.GrantUserRole;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Commands.Admin;

public class GrantUserRoleCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GrantUserRoleCommandHandler _handler;

    public GrantUserRoleCommandHandlerTests()
    {
        _handler = new GrantUserRoleCommandHandler(
            _userRepository, Substitute.For<ILogger<GrantUserRoleCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequest_When_RoleNameIsUnknown()
    {
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new User { Id = 1 });

        Func<Task> act = () => _handler.Handle(
            new GrantUserRoleCommand { UserId = 1, Role = "Wizard" }, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_UserDoesNotExist()
    {
        _userRepository.GetByIdAsync(9, Arg.Any<CancellationToken>()).Returns((User)null);

        Func<Task> act = () => _handler.Handle(
            new GrantUserRoleCommand { UserId = 9, Role = "Moderator" }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_GrantRoleAndReturnCurrentRoles()
    {
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new User { Id = 1 });
        _userRepository.GetRolesAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Role> { Role.Reader, Role.Moderator });

        var result = await _handler.Handle(
            new GrantUserRoleCommand { UserId = 1, Role = "moderator" }, CancellationToken.None);

        await _userRepository.Received(1).GrantRoleAsync(1, Role.Moderator, Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Roles.Should().BeEquivalentTo("Reader", "Moderator");
        result.RequiresTokenRefresh.Should().BeTrue();
    }
}
