using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.SubmitChapterForReview;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class SubmitChapterForReviewCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<SubmitChapterForReviewCommandHandler> _logger =
        Substitute.For<ILogger<SubmitChapterForReviewCommandHandler>>();

    private readonly SubmitChapterForReviewCommandHandler _handler;

    public SubmitChapterForReviewCommandHandlerTests()
    {
        _handler = new SubmitChapterForReviewCommandHandler(
            _storyRepository,
            _chapterRepository,
            _volumeRepository,
            _authorContext,
            _logger);
    }

    private static Chapter CreateChapter(long storyId, ChapterStatus status, Guid publicId)
    {
        return new Chapter
        {
            Id = 1,
            StoryId = storyId,
            PublicId = publicId,
            Status = status
        };
    }

    private static Story CreateStory(long id, long authorProfileId, Guid publicId)
    {
        return new Story
        {
            Id = id,
            AuthorProfileId = authorProfileId,
            PublicId = publicId
        };
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_CallerIsNotOwner()
    {
        var publicId = Guid.NewGuid();
        var chapter = CreateChapter(storyId: 5, ChapterStatus.Draft, publicId);
        var story = CreateStory(id: 5, authorProfileId: 10, Guid.NewGuid());

        _authorContext.GetAuthorProfileId().Returns(99L);
        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new SubmitChapterForReviewCommand { ChapterId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterStatusIsAlreadyPublished()
    {
        var publicId = Guid.NewGuid();
        var chapter = CreateChapter(storyId: 5, ChapterStatus.Published, publicId);
        var story = CreateStory(id: 5, authorProfileId: 10, Guid.NewGuid());

        _authorContext.GetAuthorProfileId().Returns(10L);
        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new SubmitChapterForReviewCommand { ChapterId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_PrimaryGenreMissingOrDuplicated()
    {
        // The repository collapses "missing" and "duplicate" primary genre into the
        // same HasExactlyOnePrimaryGenreAsync == false result; the handler cannot
        // distinguish them and neither can this test.
        var publicId = Guid.NewGuid();
        var chapter = CreateChapter(storyId: 5, ChapterStatus.Draft, publicId);
        var story = CreateStory(id: 5, authorProfileId: 10, Guid.NewGuid());

        _authorContext.GetAuthorProfileId().Returns(10L);
        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _storyRepository.HasExactlyOnePrimaryGenreAsync(5, Arg.Any<CancellationToken>()).Returns(false);

        var command = new SubmitChapterForReviewCommand { ChapterId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_ClearRejectionReasonAndScheduledAt_When_Resubmitting()
    {
        var publicId = Guid.NewGuid();
        var chapter = CreateChapter(storyId: 5, ChapterStatus.Rejected, publicId);
        chapter.RejectionReason = "Needs fixes";
        chapter.ScheduledAt = DateTime.UtcNow.AddDays(1);
        var story = CreateStory(id: 5, authorProfileId: 10, Guid.NewGuid());

        _authorContext.GetAuthorProfileId().Returns(10L);
        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _storyRepository.HasExactlyOnePrimaryGenreAsync(5, Arg.Any<CancellationToken>()).Returns(true);

        var command = new SubmitChapterForReviewCommand { ChapterId = publicId };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.PendingReview);
        chapter.RejectionReason.Should().BeNull();
        chapter.ScheduledAt.Should().BeNull();
    }
}
