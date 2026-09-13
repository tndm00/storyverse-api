using Content.Application.Commands.Chapters.PublishDueChapters;
using Content.Application.Interfaces.Persistence;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class PublishDueChaptersCommandHandlerTests
{
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly IFacebookPageClient _facebookPageClient = Substitute.For<IFacebookPageClient>();
    private readonly ILogger<PublishDueChaptersCommandHandler> _logger =
        Substitute.For<ILogger<PublishDueChaptersCommandHandler>>();

    private readonly PublishDueChaptersCommandHandler _handler;

    public PublishDueChaptersCommandHandlerTests()
    {
        _handler = new PublishDueChaptersCommandHandler(
            _chapterRepository, _storyRepository, _unitOfWork, _facebookPageClient, _logger);

        // Run the transactional body inline.
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<Func<CancellationToken, Task>>(0).Invoke(CancellationToken.None));

        _storyRepository.GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(ci => new Story { Id = ci.Arg<long>(), Slug = $"story-{ci.Arg<long>()}" });
    }

    private static Chapter DueChapter(long id, long storyId) => new()
    {
        Id = id,
        StoryId = storyId,
        Status = ChapterStatus.Scheduled,
        ScheduledAt = DateTime.UtcNow.AddMinutes(-1)
    };

    [Fact]
    public async Task Handle_Should_DoNothing_When_NoChaptersAreDue()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Chapter>());

        var result = await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        result.PublishedCount.Should().Be(0);
        await _chapterRepository.DidNotReceive()
            .TryMarkPublishedAsync(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_QueryWithTheRequestedBatchSize()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Chapter>());

        await _handler.Handle(new PublishDueChaptersCommand { BatchSize = 25 }, CancellationToken.None);

        await _chapterRepository.Received(1)
            .GetDueScheduledAsync(Arg.Any<DateTime>(), 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PublishEachClaimedChapterAndTransitionItsStory()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { DueChapter(1, 10), DueChapter(2, 20) });
        _chapterRepository.TryMarkPublishedAsync(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        result.PublishedCount.Should().Be(2);
        result.DueCount.Should().Be(2);
        await _storyRepository.Received(1)
            .TryStartOngoingOnFirstChapterAsync(10, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _storyRepository.Received(1)
            .TryStartOngoingOnFirstChapterAsync(20, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SkipStoryTransition_When_AnotherInstanceClaimedTheChapter()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { DueChapter(1, 10) });
        _chapterRepository.TryMarkPublishedAsync(1, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        result.PublishedCount.Should().Be(0);
        result.DueCount.Should().Be(1);
        await _storyRepository.DidNotReceive()
            .TryStartOngoingOnFirstChapterAsync(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PostToFacebook_ForEachPublishedChapter()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { DueChapter(1, 10), DueChapter(2, 20) });
        _chapterRepository.TryMarkPublishedAsync(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        await _facebookPageClient.Received(2)
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotPostToFacebook_When_ChapterWasNotClaimedByThisInstance()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { DueChapter(1, 10) });
        _chapterRepository.TryMarkPublishedAsync(1, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        await _facebookPageClient.DidNotReceive()
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_KeepPublishingRemainingChapters_When_FacebookPostThrows()
    {
        _chapterRepository.GetDueScheduledAsync(Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { DueChapter(1, 10), DueChapter(2, 20) });
        _chapterRepository.TryMarkPublishedAsync(Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _facebookPageClient
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("facebook down"));

        var result = await _handler.Handle(new PublishDueChaptersCommand(), CancellationToken.None);

        result.PublishedCount.Should().Be(2);
    }
}
