using Authentication.Application.Interfaces.Repositories;
using Authentication.Application.Interfaces.Services;
using Authentication.Application.Queries.MyAuthorProfile;
using Authentication.Domain.Entities;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Authentication.UnitTests.Queries.MyAuthorProfile;

public class GetMyAuthorProfileQueryHandlerTests
{
    private readonly IAuthorProfileRepository _authorProfileRepository = Substitute.For<IAuthorProfileRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly GetMyAuthorProfileQueryHandler _handler;

    public GetMyAuthorProfileQueryHandlerTests()
    {
        _handler = new GetMyAuthorProfileQueryHandler(_authorProfileRepository, _currentUserService);
        _currentUserService.GetUserId().Returns(1L);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CallerHasNoAuthorProfile()
    {
        _authorProfileRepository.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns((AuthorProfile)null);

        Func<Task> act = () => _handler.Handle(new GetMyAuthorProfileQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnAuthorProfile_When_CallerHasOne()
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
        _authorProfileRepository.GetByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(profile);

        var result = await _handler.Handle(new GetMyAuthorProfileQuery(), CancellationToken.None);

        result.AuthorProfileId.Should().Be(5);
        result.UserId.Should().Be(1);
        result.PenName.Should().Be("Pen Name");
        result.Verified.Should().BeTrue();
        result.RequiresTokenRefresh.Should().BeFalse();
    }
}
