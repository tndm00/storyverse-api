using Community.Application.Commands.Ratings.UpsertRating;
using Community.Application.Interfaces.Persistence;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Ratings;

public class UpsertRatingCommandHandlerTests
{
    private readonly IRatingRepository _ratingRepository = Substitute.For<IRatingRepository>();
    private readonly ICommunityUnitOfWork _unitOfWork = Substitute.For<ICommunityUnitOfWork>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly IContentRatingSyncClient _contentRatingSyncClient = Substitute.For<IContentRatingSyncClient>();
    private readonly ILogger<UpsertRatingCommandHandler> _logger =
        Substitute.For<ILogger<UpsertRatingCommandHandler>>();

    private readonly UpsertRatingCommandHandler _handler;

    public UpsertRatingCommandHandlerTests()
    {
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
        _ratingRepository.GetAggregateByStoryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((0m, 0));

        _handler = new UpsertRatingCommandHandler(
            _ratingRepository, _unitOfWork, _userContext, _contentRatingSyncClient, _logger);
    }

    [Fact]
    public async Task Handle_Should_InsertNewRating_When_NoExistingRatingForUser()
    {
        const long userId = 10;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns((Rating)null);

        var command = new UpsertRatingCommand { StoryId = storyId, Score = 5, ReviewText = "Loved it" };

        var result = await _handler.Handle(command, CancellationToken.None);

        await _ratingRepository.Received(1).AddAsync(
            Arg.Is<Rating>(r => r.StoryId == storyId && r.UserId == userId && r.Score == 5),
            Arg.Any<CancellationToken>());
        _ratingRepository.DidNotReceive().Update(Arg.Any<Rating>());
        result.Score.Should().Be(5);
        result.ReviewText.Should().Be("Loved it");
    }

    [Fact]
    public async Task Handle_Should_UpdateExistingRating_When_UserAlreadyRatedStory()
    {
        const long userId = 11;
        var storyId = Guid.NewGuid();
        var existing = new Rating { StoryId = storyId, UserId = userId, Score = 2, ReviewText = "meh" };
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new UpsertRatingCommand { StoryId = storyId, Score = 4, ReviewText = "Better now" };

        var result = await _handler.Handle(command, CancellationToken.None);

        await _ratingRepository.DidNotReceive().AddAsync(Arg.Any<Rating>(), Arg.Any<CancellationToken>());
        _ratingRepository.Received(1).Update(existing);
        existing.Score.Should().Be(4);
        existing.UpdatedAt.Should().NotBeNull();
        result.Score.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Should_StoreNullReviewText_When_ReviewTextIsWhitespace()
    {
        const long userId = 12;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns((Rating)null);

        var command = new UpsertRatingCommand { StoryId = storyId, Score = 3, ReviewText = "   " };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ReviewText.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_SyncRecomputedAggregate_When_RatingSaved()
    {
        const long userId = 13;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _ratingRepository.GetByStoryAndUserAsync(storyId, userId, Arg.Any<CancellationToken>())
            .Returns((Rating)null);
        _ratingRepository.GetAggregateByStoryAsync(storyId, Arg.Any<CancellationToken>())
            .Returns((4m, 2));

        await _handler.Handle(new UpsertRatingCommand { StoryId = storyId, Score = 5 }, CancellationToken.None);

        await _contentRatingSyncClient.Received(1).SyncRatingSummaryAsync(
            storyId, 4m, 2, Arg.Any<CancellationToken>());
    }
}
