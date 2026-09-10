using Be.StoryVerse.Core.Exceptions;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Application.Queries.Ratings.GetMyRating;
using Community.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Queries.Ratings;

public class GetMyRatingQueryHandlerTests
{
    private readonly IRatingRepository _ratingRepository = Substitute.For<IRatingRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();

    private readonly GetMyRatingQueryHandler _handler;

    public GetMyRatingQueryHandlerTests()
    {
        _handler = new GetMyRatingQueryHandler(_ratingRepository, _userContext);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserHasNotRatedStory()
    {
        const long userId = 30;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns((Rating)null);

        var query = new GetMyRatingQuery { StoryId = storyId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedDto_When_UserHasRatedStory()
    {
        const long userId = 31;
        var storyId = Guid.NewGuid();
        var rating = new Rating { StoryId = storyId, UserId = userId, Score = 5, ReviewText = "great" };
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns(rating);

        var query = new GetMyRatingQuery { StoryId = storyId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.StoryId.Should().Be(storyId);
        result.Score.Should().Be(5);
        result.ReviewText.Should().Be("great");
    }
}
