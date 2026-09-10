using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.ScheduleChapter;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class ScheduleChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<ScheduleChapterCommandHandler> _logger =
        Substitute.For<ILogger<ScheduleChapterCommandHandler>>();

    private readonly ScheduleChapterCommandHandler _handler;

    public ScheduleChapterCommandHandlerTests()
    {
        _handler = new ScheduleChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ScheduleChapter_When_ChapterIsDraft()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = ChapterStatus.Draft };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new ScheduleChapterCommand { ChapterId = chapterPublicId, ScheduledAt = scheduledAt };

        var result = await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Scheduled);
        chapter.ScheduledAt.Should().Be(scheduledAt);
        result.Status.Should().Be(ChapterStatus.Scheduled.ToString());
        _chapterRepository.Received(1).Update(chapter);
    }

    [Theory]
    [InlineData(ChapterStatus.Scheduled)]
    [InlineData(ChapterStatus.PendingReview)]
    [InlineData(ChapterStatus.InReview)]
    [InlineData(ChapterStatus.Published)]
    [InlineData(ChapterStatus.Rejected)]
    [InlineData(ChapterStatus.Removed)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterCannotBeScheduled(ChapterStatus status)
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = status };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new ScheduleChapterCommand
        {
            ChapterId = chapterPublicId,
            ScheduledAt = DateTime.UtcNow.AddDays(1)
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
