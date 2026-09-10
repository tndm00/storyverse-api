using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Queries.UsersDirectory;
using Authentication.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Queries.UsersDirectory;

public class GetUsersDirectoryQueryHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly GetUsersDirectoryQueryHandler _handler;

    public GetUsersDirectoryQueryHandlerTests()
    {
        _handler = new GetUsersDirectoryQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmpty_When_NoValidIds()
    {
        var result = await _handler.Handle(
            new GetUsersDirectoryQuery { UserIds = new long[] { 0, -3 } }, CancellationToken.None);

        result.Should().BeEmpty();
        await _userRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapDistinctIdsToDisplayNames()
    {
        _userRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>
            {
                new() { Id = 1, DisplayName = "Ann" },
                new() { Id = 2, DisplayName = "Bob" }
            });

        var result = await _handler.Handle(
            new GetUsersDirectoryQuery { UserIds = new long[] { 1, 2, 2, 1 } }, CancellationToken.None);

        result.Should().BeEquivalentTo(new[]
        {
            new { UserId = 1L, DisplayName = "Ann" },
            new { UserId = 2L, DisplayName = "Bob" }
        });
    }
}
