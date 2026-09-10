using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.RejectChapter;
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

public class RejectChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly IChapterReviewActionRepository _reviewActionRepository =
        Substitute.For<IChapterReviewActionRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly ICurrentAuthorContext _currentUser = Substitute.For<ICurrentAuthorContext>();
    private readonly INotificationServiceClient _notificationClient = Substitute.For<INotificationServiceClient>();
    private readonly IAuthorDirectoryClient _authorDirectory = Substitute.For<IAuthorDirectoryClient>();
    private readonly ILogger<RejectChapterCommandHandler> _logger =
        Substitute.For<ILogger<RejectChapterCommandHandler>>();

    private readonly RejectChapterCommandHandler _handler;

    public RejectChapterCommandHandlerTests()
    {
        _handler = new RejectChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _reviewActionRepository,
            _unitOfWork, _currentUser, _notificationClient, _authorDirectory, _logger);

        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));

        _authorDirectory.GetAuthorUserIdAsync(42, Arg.Any<CancellationToken>()).Returns(7L);
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

        var command = new RejectChapterCommand { ChapterId = publicId, Reason = "Not good enough" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_TrimAndStoreReason_When_Rejecting()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid() };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new RejectChapterCommand { ChapterId = publicId, Reason = "  Needs more detail  " };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Rejected);
        chapter.RejectionReason.Should().Be("Needs more detail");
    }

    [Fact]
    public async Task Handle_Should_SendChapterRejectedNotification_WithReasonInBody()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 3, StoryId = 5, PublicId = publicId, Title = "Ch 2", Status = ChapterStatus.InReview
        };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), Title = "S", AuthorProfileId = 42 };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        await _handler.Handle(
            new RejectChapterCommand { ChapterId = publicId, Reason = "  Too short  " }, CancellationToken.None);

        await _notificationClient.Received(1).SendAsync(
            7,
            NotificationKind.ChapterRejected,
            Arg.Any<string>(),
            Arg.Is<string>(b => b.Contains("Too short")),
            "Chapter",
            publicId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotFailReject_When_NotificationThrows()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 4, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 42 };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _notificationClient
            .When(x => x.SendAsync(Arg.Any<long>(), Arg.Any<NotificationKind>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("down"));

        var act = () => _handler.Handle(
            new RejectChapterCommand { ChapterId = publicId, Reason = "bad" }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        chapter.Status.Should().Be(ChapterStatus.Rejected);
    }
}
