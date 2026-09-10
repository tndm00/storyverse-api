using Community.Application.Commands.Votes.CastVote;
using Community.Application.Interfaces.Repositories;
using Community.Application.Interfaces.Services;
using Community.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Community.UnitTests.Commands.Votes;

public class CastVoteCommandHandlerTests
{
    private readonly IVoteRepository _voteRepository = Substitute.For<IVoteRepository>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<CastVoteCommandHandler> _logger = Substitute.For<ILogger<CastVoteCommandHandler>>();

    private readonly CastVoteCommandHandler _handler;

    public CastVoteCommandHandlerTests()
    {
        _handler = new CastVoteCommandHandler(_voteRepository, _userContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_RecordVote_When_FirstVoteInCurrentWeek()
    {
        const long userId = 20;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _voteRepository.ExistsAsync(storyId, userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _voteRepository.CountForStoryWeekAsync(storyId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CastVoteCommand { StoryId = storyId };

        var result = await _handler.Handle(command, CancellationToken.None);

        await _voteRepository.Received(1).AddAsync(
            Arg.Is<Vote>(v => v.StoryId == storyId && v.UserId == userId),
            Arg.Any<CancellationToken>());
        await _voteRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Recorded.Should().BeTrue();
        result.WeekVoteCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_NotRecordSecondVote_When_AlreadyVotedInSameIsoWeek()
    {
        const long userId = 21;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _voteRepository.ExistsAsync(storyId, userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _voteRepository.CountForStoryWeekAsync(storyId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var command = new CastVoteCommand { StoryId = storyId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _voteRepository.DidNotReceive().AddAsync(Arg.Any<Vote>(), Arg.Any<CancellationToken>());
        await _voteRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Recorded.Should().BeFalse();
        result.WeekVoteCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_ReturnCurrentWeekVoteCount_When_VoteIsCast()
    {
        const long userId = 22;
        var storyId = Guid.NewGuid();
        _userContext.GetUserId().Returns(userId);
        _voteRepository.ExistsAsync(storyId, userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _voteRepository.CountForStoryWeekAsync(storyId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(7);

        var command = new CastVoteCommand { StoryId = storyId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.StoryId.Should().Be(storyId);
        result.WeekVoteCount.Should().Be(7);
        result.WeekKey.Should().NotBeNullOrEmpty();
    }
}
