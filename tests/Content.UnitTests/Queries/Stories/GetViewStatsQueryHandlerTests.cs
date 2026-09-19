using Be.StoryVerse.Cache.ViewTracking;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Stories.GetViewStats;
using Content.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetViewStatsQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IViewStatsReader _viewStatsReader = Substitute.For<IViewStatsReader>();

    private readonly GetViewStatsQueryHandler _handler;

    public GetViewStatsQueryHandlerTests()
    {
        _handler = new GetViewStatsQueryHandler(_storyRepository, _viewStatsReader);
    }

    [Fact]
    public async Task Handle_Should_AddPendingRedisViewsToPostgresTotal()
    {
        _storyRepository.SumViewCountAsync(Arg.Any<CancellationToken>()).Returns(312L);
        _viewStatsReader.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ViewStatsSnapshot { IsAvailable = true, PendingStoryViews = 8, TodayViews = 20, YesterdayViews = 15 });

        var result = await _handler.Handle(new GetViewStatsQuery(), CancellationToken.None);

        result.TotalViews.Should().Be(320);
        result.TodayViews.Should().Be(20);
        result.YesterdayViews.Should().Be(15);
    }

    [Fact]
    public async Task Handle_Should_AskForTheTop10StoriesOfTheDay()
    {
        _storyRepository.SumViewCountAsync(Arg.Any<CancellationToken>()).Returns(0L);
        _viewStatsReader.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ViewStatsSnapshot { IsAvailable = true });

        await _handler.Handle(new GetViewStatsQuery(), CancellationToken.None);

        await _viewStatsReader.Received(1).GetSnapshotAsync(10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNullDailyFigures_When_RedisIsUnavailable()
    {
        _storyRepository.SumViewCountAsync(Arg.Any<CancellationToken>()).Returns(312L);
        _viewStatsReader.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ViewStatsSnapshot { IsAvailable = false });

        var result = await _handler.Handle(new GetViewStatsQuery(), CancellationToken.None);

        result.TotalViews.Should().Be(312);
        result.TodayViews.Should().BeNull();
        result.YesterdayViews.Should().BeNull();
        result.TopStoriesToday.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_MapTopStoriesInRankingOrderWithTheirViews()
    {
        _storyRepository.SumViewCountAsync(Arg.Any<CancellationToken>()).Returns(0L);
        _viewStatsReader.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ViewStatsSnapshot
            {
                IsAvailable = true,
                TopStoriesToday = new[]
                {
                    new TopStoryViews { StoryId = 22, Views = 9 },
                    new TopStoryViews { StoryId = 7, Views = 4 }
                }
            });
        _storyRepository.GetByIdsInOrderAsync(Arg.Any<IReadOnlyList<long>>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new Story { Id = 22, Title = "Tầng 17 Không Ai Ở", Slug = "tang-17-khong-ai-o" },
                new Story { Id = 7, Title = "Phòng 313", Slug = "phong-313" }
            });

        var result = await _handler.Handle(new GetViewStatsQuery(), CancellationToken.None);

        result.TopStoriesToday.Should().HaveCount(2);
        result.TopStoriesToday[0].Title.Should().Be("Tầng 17 Không Ai Ở");
        result.TopStoriesToday[0].Views.Should().Be(9);
        result.TopStoriesToday[1].Slug.Should().Be("phong-313");
        result.TopStoriesToday[1].Views.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Should_OmitDeletedStoriesFromTopList()
    {
        _storyRepository.SumViewCountAsync(Arg.Any<CancellationToken>()).Returns(0L);
        _viewStatsReader.GetSnapshotAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ViewStatsSnapshot
            {
                IsAvailable = true,
                TopStoriesToday = new[]
                {
                    new TopStoryViews { StoryId = 22, Views = 9 },
                    new TopStoryViews { StoryId = 99, Views = 5 }
                }
            });
        // Story 99 was deleted after it was ranked: the repository simply does not return it.
        _storyRepository.GetByIdsInOrderAsync(Arg.Any<IReadOnlyList<long>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new Story { Id = 22, Title = "Tầng 17 Không Ai Ở", Slug = "tang-17-khong-ai-o" } });

        var result = await _handler.Handle(new GetViewStatsQuery(), CancellationToken.None);

        result.TopStoriesToday.Should().ContainSingle().Which.Views.Should().Be(9);
    }
}
