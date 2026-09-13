using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.ApproveChapter;
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

public class ApproveChapterCommandHandlerTests
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
    private readonly IFacebookPageClient _facebookPageClient = Substitute.For<IFacebookPageClient>();
    private readonly ILogger<ApproveChapterCommandHandler> _logger =
        Substitute.For<ILogger<ApproveChapterCommandHandler>>();

    private readonly ApproveChapterCommandHandler _handler;

    public ApproveChapterCommandHandlerTests()
    {
        _handler = new ApproveChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _reviewActionRepository,
            _unitOfWork, _currentUser, _notificationClient, _authorDirectory, _facebookPageClient, _logger);

        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));

        // Author profile 42 maps to user id 7 (Id != UserId, the case this feature exists for).
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

    [Fact]
    public async Task Handle_Should_SendChapterApprovedNotification_ToAuthor_AfterCommit()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 7, StoryId = 5, PublicId = publicId, Title = "Ch 1", Status = ChapterStatus.InReview
        };
        var story = new Story
        {
            Id = 5, PublicId = Guid.NewGuid(), Title = "My Story", AuthorProfileId = 42, Status = StoryStatus.Ongoing
        };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        await _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await _authorDirectory.Received(1).GetAuthorUserIdAsync(42, Arg.Any<CancellationToken>());
        await _notificationClient.Received(1).SendAsync(
            7,
            NotificationKind.ChapterApproved,
            Arg.Any<string>(),
            Arg.Any<string>(),
            "Chapter",
            publicId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotSendNotification_When_AuthorUserIdCannotBeResolved()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 10, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 99, Status = StoryStatus.Ongoing };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorDirectory.GetAuthorUserIdAsync(99, Arg.Any<CancellationToken>()).Returns((long?)null);

        await _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Published);
        await _notificationClient.DidNotReceive().SendAsync(
            Arg.Any<long>(), Arg.Any<NotificationKind>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotFailApprove_When_AuthorLookupThrows()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 11, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 42, Status = StoryStatus.Ongoing };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorDirectory
            .When(x => x.GetAuthorUserIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("auth service down"));

        var act = () => _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        chapter.Status.Should().Be(ChapterStatus.Published);
        await _notificationClient.DidNotReceive().SendAsync(
            Arg.Any<long>(), Arg.Any<NotificationKind>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotFailApprove_When_NotificationThrows()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 8, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 42, Status = StoryStatus.Ongoing };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _notificationClient
            .When(x => x.SendAsync(Arg.Any<long>(), Arg.Any<NotificationKind>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("notification service down"));

        var act = () => _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        chapter.Status.Should().Be(ChapterStatus.Published);
    }

    [Fact]
    public async Task Handle_Should_PostNewStoryAnnouncement_When_FirstChapterApproved()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 12, StoryId = 5, PublicId = publicId, Title = "Ch 1", OrderIndex = 1m, Status = ChapterStatus.InReview
        };
        var story = new Story
        {
            Id = 5, PublicId = Guid.NewGuid(), Title = "Nhà hoang", Slug = "nha-hoang",
            Description = "Một câu chuyện rùng rợn.", Status = StoryStatus.Draft
        };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        await _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await _facebookPageClient.Received(1).PostAsync(
            Arg.Is<string>(m => m.Contains("Nhà hoang") && m.Contains("Một câu chuyện rùng rợn.")),
            Arg.Is<string>(l => l.EndsWith("/truyen/nha-hoang")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PostChapterUpdateAnnouncement_When_LaterChapterApproved()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 13, StoryId = 5, PublicId = publicId, Title = "Bóng trong đêm", OrderIndex = 3m,
            Status = ChapterStatus.InReview
        };
        var story = new Story
        {
            Id = 5, PublicId = Guid.NewGuid(), Title = "Nhà hoang", Slug = "nha-hoang", Status = StoryStatus.Ongoing
        };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        await _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await _facebookPageClient.Received(1).PostAsync(
            Arg.Is<string>(m => m.Contains("Nhà hoang") && m.Contains("Bóng trong đêm")),
            Arg.Is<string>(l => l.EndsWith("/truyen/nha-hoang/chuong/3")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotFailApprove_When_FacebookPostThrows()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 14, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), Slug = "s", Status = StoryStatus.Ongoing };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _facebookPageClient
            .PostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("facebook down"));

        var act = () => _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        chapter.Status.Should().Be(ChapterStatus.Published);
    }

    [Fact]
    public async Task Handle_Should_NotSendNotification_When_GuestAuthor()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 9, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story
        {
            Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 0, GuestAuthorName = "Anon", Status = StoryStatus.Ongoing
        };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        await _handler.Handle(new ApproveChapterCommand { ChapterId = publicId }, CancellationToken.None);

        await _notificationClient.DidNotReceive().SendAsync(
            Arg.Any<long>(), Arg.Any<NotificationKind>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
