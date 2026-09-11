using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Ratings.SetStoryRatingSummary;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Ratings;

public class SetStoryRatingSummaryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly SetStoryRatingSummaryCommandHandler _handler;

    public SetStoryRatingSummaryCommandHandlerTests()
    {
        _handler = new SetStoryRatingSummaryCommandHandler(
            _storyRepository, Substitute.For<ILogger<SetStoryRatingSummaryCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_StoryMissing()
    {
        _storyRepository.GetByPublicIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Story)null);

        Func<Task> act = () => _handler.Handle(
            new SetStoryRatingSummaryCommand { StoryId = Guid.NewGuid(), RatingAvg = 4m, RatingCount = 2 },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateRatingAvgAndCount_When_StoryExists()
    {
        var id = Guid.NewGuid();
        var story = new Story { PublicId = id, RatingAvg = 0m, RatingCount = 0 };
        _storyRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(story);

        var result = await _handler.Handle(
            new SetStoryRatingSummaryCommand { StoryId = id, RatingAvg = 4m, RatingCount = 2 },
            CancellationToken.None);

        story.RatingAvg.Should().Be(4m);
        story.RatingCount.Should().Be(2);
        result.RatingAvg.Should().Be(4m);
        result.RatingCount.Should().Be(2);
        _storyRepository.Received(1).Update(story);
        await _storyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RoundRatingAvg_When_MoreThanTwoDecimals()
    {
        var id = Guid.NewGuid();
        var story = new Story { PublicId = id };
        _storyRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(story);

        var result = await _handler.Handle(
            new SetStoryRatingSummaryCommand { StoryId = id, RatingAvg = 4.666667m, RatingCount = 3 },
            CancellationToken.None);

        result.RatingAvg.Should().Be(4.67m);
    }
}
