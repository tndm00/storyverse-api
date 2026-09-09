using Authentication.Application.Commands.CreateAuthorProfile;
using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Commands;

public class CreateAuthorProfileCommandHandlerTests
{
    private readonly IAuthorProfileRepository _authorProfileRepository = Substitute.For<IAuthorProfileRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<CreateAuthorProfileCommandHandler> _logger =
        Substitute.For<ILogger<CreateAuthorProfileCommandHandler>>();

    private readonly CreateAuthorProfileCommandHandler _handler;

    public CreateAuthorProfileCommandHandlerTests()
    {
        _handler = new CreateAuthorProfileCommandHandler(
            _authorProfileRepository, _userRepository, _currentUserService, _logger);

        _currentUserService.GetUserId().Returns(1L);
    }

    private static CreateAuthorProfileCommand CreateCommand()
    {
        return new CreateAuthorProfileCommand { PenName = "Pen Name" };
    }

    [Theory]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Deleted)]
    public async Task Handle_Should_ThrowBadRequestException_When_UserIsNotActive(UserStatus status)
    {
        var user = new User { Id = 1, Status = status };
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserDoesNotExist()
    {
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((User)null);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_UserAlreadyHasAuthorProfile()
    {
        var user = new User { Id = 1, Status = UserStatus.Active };
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _authorProfileRepository.ExistsByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = () => _handler.Handle(CreateCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_Should_CreateProfileAndGrantAuthorRole_When_ActiveUserHasNoExistingProfile()
    {
        var user = new User { Id = 1, Status = UserStatus.Active };
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _authorProfileRepository.ExistsByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.PenName.Should().Be("Pen Name");
        result.RequiresTokenRefresh.Should().BeTrue();
        await _authorProfileRepository.Received(1).AddAsync(Arg.Any<AuthorProfile>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).GrantRoleAsync(1, Role.Author, Arg.Any<CancellationToken>());
    }
}
