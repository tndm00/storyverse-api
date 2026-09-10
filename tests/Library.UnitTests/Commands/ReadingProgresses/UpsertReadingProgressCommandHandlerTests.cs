using FluentAssertions;
using Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Commands.ReadingProgresses;

public class UpsertReadingProgressCommandHandlerTests
{
    private readonly IReadingProgressRepository _readingProgressRepository =
        Substitute.For<IReadingProgressRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<UpsertReadingProgressCommandHandler> _logger =
        Substitute.For<ILogger<UpsertReadingProgressCommandHandler>>();

    private readonly UpsertReadingProgressCommandHandler _handler;

    public UpsertReadingProgressCommandHandlerTests()
    {
        _handler = new UpsertReadingProgressCommandHandler(_readingProgressRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_InsertNewProgress_When_NoneExistsForUserAndStory()
    {
        var storyId = Guid.NewGuid();
        var chapterId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>())
            .Returns((ReadingProgress)null);

        var command = new UpsertReadingProgressCommand
        {
            StoryId = storyId,
            LastChapterId = chapterId,
            ScrollPercent = 25m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.StoryId.Should().Be(storyId);
        result.LastChapterId.Should().Be(chapterId);
        await _readingProgressRepository.Received(1).AddAsync(
            Arg.Is<ReadingProgress>(p => p.UserId == 1L && p.StoryId == storyId && p.LastChapterId == chapterId),
            Arg.Any<CancellationToken>());
        _readingProgressRepository.DidNotReceive().Update(Arg.Any<ReadingProgress>());
        await _readingProgressRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateExistingProgress_When_OneAlreadyExistsForUserAndStory()
    {
        var storyId = Guid.NewGuid();
        var oldChapterId = Guid.NewGuid();
        var newChapterId = Guid.NewGuid();
        var progress = new ReadingProgress
        {
            Id = 1,
            UserId = 1L,
            StoryId = storyId,
            LastChapterId = oldChapterId,
            LastReadAt = DateTime.UtcNow.AddDays(-1)
        };
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(progress);

        var command = new UpsertReadingProgressCommand
        {
            StoryId = storyId,
            LastChapterId = newChapterId,
            ScrollPercent = 80m
        };

        await _handler.Handle(command, CancellationToken.None);

        progress.LastChapterId.Should().Be(newChapterId);
        progress.ScrollPercent.Should().Be(80m);
        progress.UpdatedAt.Should().NotBeNull();
        await _readingProgressRepository.DidNotReceive().AddAsync(
            Arg.Any<ReadingProgress>(), Arg.Any<CancellationToken>());
        _readingProgressRepository.Received(1).Update(progress);
        await _readingProgressRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
