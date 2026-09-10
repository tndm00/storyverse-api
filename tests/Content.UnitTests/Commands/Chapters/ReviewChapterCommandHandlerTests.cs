using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.ReviewChapter;
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

public class ReviewChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly IChapterReviewActionRepository _reviewActionRepository =
        Substitute.For<IChapterReviewActionRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly ICurrentAuthorContext _currentUser = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<ReviewChapterCommandHandler> _logger =
        Substitute.For<ILogger<ReviewChapterCommandHandler>>();

    private readonly ReviewChapterCommandHandler _handler;

    public ReviewChapterCommandHandlerTests()
    {
        _handler = new ReviewChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _reviewActionRepository,
            _unitOfWork, _currentUser, _logger);

        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_NotCheckOwnership_When_MovingChapterIntoReview()
    {
        // A moderator reviewing someone else's chapter must not be blocked by
        // ownership: the handler never touches ICurrentAuthorContext.GetAuthorProfileId().
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = ChapterStatus.PendingReview };
        var story = new Story { Id = 5, AuthorProfileId = 123, PublicId = Guid.NewGuid() };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new ReviewChapterCommand { ChapterId = publicId };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.InReview);
        _currentUser.DidNotReceive().GetAuthorProfileId();
    }

    [Theory]
    [InlineData(ChapterStatus.Draft)]
    [InlineData(ChapterStatus.InReview)]
    [InlineData(ChapterStatus.Published)]
    [InlineData(ChapterStatus.Rejected)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterIsNotPendingReview(ChapterStatus status)
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = status };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);

        var command = new ReviewChapterCommand { ChapterId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
