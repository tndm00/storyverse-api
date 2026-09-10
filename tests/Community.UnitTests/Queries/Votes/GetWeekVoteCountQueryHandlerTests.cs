using Community.Application.Interfaces.Repositories;
using Community.Application.Queries.Votes.GetWeekVoteCount;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Queries.Votes;

public class GetWeekVoteCountQueryHandlerTests
{
    private readonly IVoteRepository _voteRepository = Substitute.For<IVoteRepository>();

    private readonly GetWeekVoteCountQueryHandler _handler;

    public GetWeekVoteCountQueryHandlerTests()
    {
        _handler = new GetWeekVoteCountQueryHandler(_voteRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnCurrentWeekVoteCount_When_Called()
    {
        var storyId = Guid.NewGuid();
        _voteRepository.CountForStoryWeekAsync(storyId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(15);

        var query = new GetWeekVoteCountQuery { StoryId = storyId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.StoryId.Should().Be(storyId);
        result.WeekVoteCount.Should().Be(15);
        result.WeekKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_Should_ReturnZero_When_NoVotesCastThisWeek()
    {
        var storyId = Guid.NewGuid();
        _voteRepository.CountForStoryWeekAsync(storyId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetWeekVoteCountQuery { StoryId = storyId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.WeekVoteCount.Should().Be(0);
    }
}
