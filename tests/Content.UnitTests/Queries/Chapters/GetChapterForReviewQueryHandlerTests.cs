using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Chapters.GetChapterForReview;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetChapterForReviewQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly IChapterReviewActionRepository _reviewActionRepository =
        Substitute.For<IChapterReviewActionRepository>();

    private readonly GetChapterForReviewQueryHandler _handler;

    public GetChapterForReviewQueryHandlerTests()
    {
        _reviewActionRepository.GetByChapterIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ChapterReviewAction>());

        _handler = new GetChapterForReviewQueryHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _reviewActionRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnChapterDetail_When_ChapterIsPendingReview_RegardlessOfCaller()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 1,
            StoryId = 5,
            PublicId = chapterPublicId,
            Title = "Ch 1",
            Status = ChapterStatus.PendingReview
        };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var query = new GetChapterForReviewQuery { ChapterId = chapterPublicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(chapterPublicId);
        result.StoryId.Should().Be(story.PublicId);
        result.Status.Should().Be(ChapterStatus.PendingReview.ToString());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ChapterDoesNotExist()
    {
        var chapterPublicId = Guid.NewGuid();
        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns((Chapter)null);

        var query = new GetChapterForReviewQuery { ChapterId = chapterPublicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
