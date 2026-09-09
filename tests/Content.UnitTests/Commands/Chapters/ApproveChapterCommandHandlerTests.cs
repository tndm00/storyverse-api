using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.ApproveChapter;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class ApproveChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ILogger<ApproveChapterCommandHandler> _logger =
        Substitute.For<ILogger<ApproveChapterCommandHandler>>();

    private readonly ApproveChapterCommandHandler _handler;

    public ApproveChapterCommandHandlerTests()
    {
        _handler = new ApproveChapterCommandHandler(_storyRepository, _chapterRepository, _volumeRepository, _logger);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft)]
    [InlineData(ChapterStatus.PendingReview)]
    [InlineData(ChapterStatus.Published)]
    [InlineData(ChapterStatus.Rejected)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterIsNotInReview(ChapterStatus status)
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = status };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);

        var command = new ApproveChapterCommand { ChapterId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_FlipStoryToOngoingAndSetPublishedAt_When_FirstChapterApproved()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), Status = StoryStatus.Draft, PublishedAt = null };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new ApproveChapterCommand { ChapterId = publicId };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Published);
        chapter.PublishedAt.Should().NotBeNull();
        story.Status.Should().Be(StoryStatus.Ongoing);
        story.PublishedAt.Should().NotBeNull();
        _storyRepository.Received(1).Update(story);
    }

    [Fact]
    public async Task Handle_Should_NotReflipStatusOrOverwritePublishedAt_When_StoryAlreadyOngoing()
    {
        var publicId = Guid.NewGuid();
        var originalPublishedAt = DateTime.UtcNow.AddDays(-10);
        var chapter = new Chapter { Id = 2, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story
        {
            Id = 5,
            PublicId = Guid.NewGuid(),
            Status = StoryStatus.Ongoing,
            PublishedAt = originalPublishedAt
        };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new ApproveChapterCommand { ChapterId = publicId };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Published);
        story.Status.Should().Be(StoryStatus.Ongoing);
        story.PublishedAt.Should().Be(originalPublishedAt);
        _storyRepository.DidNotReceive().Update(Arg.Any<Story>());
    }
}
