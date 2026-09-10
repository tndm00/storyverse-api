using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Stories.GetStoryStatusCounts;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetStoryStatusCountsQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly GetStoryStatusCountsQueryHandler _handler;

    public GetStoryStatusCountsQueryHandlerTests()
    {
        _handler = new GetStoryStatusCountsQueryHandler(_storyRepository);
    }

    [Fact]
    public async Task Handle_Should_ProjectCountsByStatusName_And_SumTotal()
    {
        _storyRepository.CountByStatusAsync(Arg.Any<CancellationToken>())
            .Returns((IReadOnlyDictionary<StoryStatus, int>)new Dictionary<StoryStatus, int>
            {
                [StoryStatus.Draft] = 3,
                [StoryStatus.Ongoing] = 5,
                [StoryStatus.Completed] = 2
            });

        var result = await _handler.Handle(new GetStoryStatusCountsQuery(), CancellationToken.None);

        result.Total.Should().Be(10);
        result.ByStatus["Draft"].Should().Be(3);
        result.ByStatus["Ongoing"].Should().Be(5);
    }
}
