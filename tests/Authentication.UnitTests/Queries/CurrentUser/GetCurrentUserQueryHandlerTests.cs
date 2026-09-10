using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Application.Mappings;
using Authentication.Application.Queries.CurrentUser;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Mapster;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Queries.CurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly GetCurrentUserQueryHandler _handler;

    static GetCurrentUserQueryHandlerTests()
    {
        // The handler calls User.Adapt<CurrentUserResponseDto>(), which relies on
        // AuthenticationMappingConfig's Id -> UserId mapping. In production this is
        // registered by ServiceRegistration.AddApplication() at startup; unit tests
        // never run that, so register it directly against Mapster's global config.
        new AuthenticationMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public GetCurrentUserQueryHandlerTests()
    {
        _handler = new GetCurrentUserQueryHandler(_userRepository, _currentUserService);
        _currentUserService.GetUserId().Returns(1L);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserDoesNotExist()
    {
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((User)null);

        Func<Task> act = () => _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnCurrentUserWithRoles_When_UserExists()
    {
        var user = new User
        {
            Id = 1,
            Email = "reader@example.com",
            DisplayName = "Test Reader",
            AvatarUrl = "https://example.com/avatar.png",
            LastLoginAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        _userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        _userRepository.GetRolesAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Role> { Role.Reader, Role.Author });

        var result = await _handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.UserId.Should().Be(1);
        result.Email.Should().Be("reader@example.com");
        result.DisplayName.Should().Be("Test Reader");
        result.Roles.Should().BeEquivalentTo(new[] { "Reader", "Author" });
    }
}
