using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Chapters.GetPendingReviewChapters;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetPendingReviewChaptersQueryHandlerTests
{
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();

    private readonly GetPendingReviewChaptersQueryHandler _handler;

    public GetPendingReviewChaptersQueryHandlerTests()
    {
        _handler = new GetPendingReviewChaptersQueryHandler(_chapterRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedPendingReviewChapters_When_QueueHasItems()
    {
        var chapter = new Chapter { Id = 1, PublicId = Guid.NewGuid(), Title = "Ch 1", Status = ChapterStatus.PendingReview };
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), Title = "Story", Slug = "story" };

        _chapterRepository.GetPendingReviewAsync(null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<(Chapter Chapter, Story Story)> { (chapter, story) }, 1));

        var query = new GetPendingReviewChaptersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().ContainSingle(x => x.ChapterId == chapter.PublicId);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_PassNullStatus_When_StatusFilterIsNotAValidEnumValue()
    {
        ChapterStatus? capturedStatus = ChapterStatus.Draft;
        _chapterRepository.GetPendingReviewAsync(Arg.Any<ChapterStatus?>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                capturedStatus = ci.Arg<ChapterStatus?>();
                return (new List<(Chapter Chapter, Story Story)>(), 0);
            });

        var query = new GetPendingReviewChaptersQuery { Status = "NotARealStatus" };

        await _handler.Handle(query, CancellationToken.None);

        capturedStatus.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnEmpty_When_TypeIsNotChapter()
    {
        var result = await _handler.Handle(
            new GetPendingReviewChaptersQuery { Type = "Story" },
            CancellationToken.None);

        result.TotalCount.Should().Be(0);
        await _chapterRepository.DidNotReceive().GetPendingReviewAsync(
            Arg.Any<ChapterStatus?>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
