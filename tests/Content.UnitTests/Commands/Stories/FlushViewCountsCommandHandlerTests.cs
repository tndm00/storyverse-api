using Be.StoryVerse.Cache.ViewTracking;
using Content.Application.Commands.Stories.FlushViewCounts;
using Content.Application.Interfaces.Persistence;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class FlushViewCountsCommandHandlerTests
{
    private readonly IViewCountBuffer _buffer = Substitute.For<IViewCountBuffer>();
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly ILogger<FlushViewCountsCommandHandler> _logger = Substitute.For<ILogger<FlushViewCountsCommandHandler>>();

    private readonly FlushViewCountsCommandHandler _handler;

    public FlushViewCountsCommandHandlerTests()
    {
        _handler = new FlushViewCountsCommandHandler(_buffer, _storyRepository, _chapterRepository, _unitOfWork, _logger);

        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_NothingIsPending()
    {
        _buffer.TakePendingAsync(Arg.Any<CancellationToken>()).Returns(new PendingViewCounts());

        var result = await _handler.Handle(new FlushViewCountsCommand(), CancellationToken.None);

        result.StoryCount.Should().Be(0);
        await _storyRepository.DidNotReceive().AddViewCountsAsync(Arg.Any<IReadOnlyDictionary<long, long>>(), Arg.Any<CancellationToken>());
        await _buffer.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddDeltasToPostgresThenCommitTheBuffer()
    {
        var stories = new Dictionary<long, long> { [5] = 3, [9] = 1 };
        var chapters = new Dictionary<long, long> { [40] = 3 };
        _buffer.TakePendingAsync(Arg.Any<CancellationToken>())
            .Returns(new PendingViewCounts { Stories = stories, Chapters = chapters });

        var result = await _handler.Handle(new FlushViewCountsCommand(), CancellationToken.None);

        result.StoryCount.Should().Be(2);
        result.ChapterCount.Should().Be(1);
        await _storyRepository.Received(1).AddViewCountsAsync(stories, Arg.Any<CancellationToken>());
        await _chapterRepository.Received(1).AddViewCountsAsync(chapters, Arg.Any<CancellationToken>());
        await _buffer.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotCommitTheBuffer_When_PostgresUpdateFails()
    {
        _buffer.TakePendingAsync(Arg.Any<CancellationToken>())
            .Returns(new PendingViewCounts { Stories = new Dictionary<long, long> { [5] = 3 } });
        _storyRepository
            .AddViewCountsAsync(Arg.Any<IReadOnlyDictionary<long, long>>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("db down"));

        Func<Task> act = () => _handler.Handle(new FlushViewCountsCommand(), CancellationToken.None);

        // The parked batch must survive so the next tick can retry it (at-least-once).
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _buffer.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
