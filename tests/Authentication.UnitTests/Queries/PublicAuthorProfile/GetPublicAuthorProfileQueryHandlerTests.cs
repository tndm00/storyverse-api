using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Queries.PublicAuthorProfile;
using Authentication.Domain.Entities;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Queries.PublicAuthorProfile;

public class GetPublicAuthorProfileQueryHandlerTests
{
    private readonly IAuthorProfileRepository _authorProfileRepository = Substitute.For<IAuthorProfileRepository>();

    private readonly GetPublicAuthorProfileQueryHandler _handler;

    public GetPublicAuthorProfileQueryHandlerTests()
    {
        _handler = new GetPublicAuthorProfileQueryHandler(_authorProfileRepository);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_AuthorProfileDoesNotExist()
    {
        _authorProfileRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns((AuthorProfile)null);

        Func<Task> act = () => _handler.Handle(
            new GetPublicAuthorProfileQuery { AuthorProfileId = 5 }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnPublicAuthorProfile_When_AuthorProfileExists()
    {
        var profile = new AuthorProfile
        {
            Id = 5,
            UserId = 1,
            PenName = "Pen Name",
            Bio = "Bio",
            AvatarUrl = "https://example.com/avatar.png",
            BannerUrl = "https://example.com/banner.png",
            Verified = true
        };
        _authorProfileRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _handler.Handle(
            new GetPublicAuthorProfileQuery { AuthorProfileId = 5 }, CancellationToken.None);

        result.AuthorProfileId.Should().Be(5);
        result.PenName.Should().Be("Pen Name");
        result.Bio.Should().Be("Bio");
        result.Verified.Should().BeTrue();
    }
}
