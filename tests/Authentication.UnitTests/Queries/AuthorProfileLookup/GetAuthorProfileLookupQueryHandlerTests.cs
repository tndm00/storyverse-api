using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Queries.AuthorProfileLookup;
using Authentication.Domain.Entities;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Queries.AuthorProfileLookup;

public class GetAuthorProfileLookupQueryHandlerTests
{
    private readonly IAuthorProfileRepository _authorProfileRepository = Substitute.For<IAuthorProfileRepository>();

    private readonly GetAuthorProfileLookupQueryHandler _handler;

    public GetAuthorProfileLookupQueryHandlerTests()
    {
        _handler = new GetAuthorProfileLookupQueryHandler(_authorProfileRepository);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_AuthorProfileDoesNotExist()
    {
        _authorProfileRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((AuthorProfile)null);

        Func<Task> act = () => _handler.Handle(
            new GetAuthorProfileLookupQuery { AuthorProfileId = 5 }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnOwningUserId_When_AuthorProfileExists()
    {
        _authorProfileRepository.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns(new AuthorProfile { Id = 5, UserId = 71, PenName = "Pen" });

        var result = await _handler.Handle(
            new GetAuthorProfileLookupQuery { AuthorProfileId = 5 }, CancellationToken.None);

        result.AuthorProfileId.Should().Be(5);
        result.UserId.Should().Be(71);
    }
}
