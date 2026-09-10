using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Chapters.GetReviewedChapters;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetReviewedChaptersQueryHandlerTests
{
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly GetReviewedChaptersQueryHandler _handler;

    public GetReviewedChaptersQueryHandlerTests()
    {
        _handler = new GetReviewedChaptersQueryHandler(_chapterRepository);
    }

    [Theory]
    [InlineData("Rejected", ChapterReviewActionType.Rejected)]
    [InlineData("Approved", ChapterReviewActionType.Approved)]
    [InlineData("", ChapterReviewActionType.Approved)]
    public async Task Handle_Should_MapStatusToActionType(string status, ChapterReviewActionType expected)
    {
        ChapterReviewActionType? captured = null;
        _chapterRepository.GetReviewedAsync(Arg.Any<ChapterReviewActionType>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<ChapterReviewActionType>();
                return ((IReadOnlyList<(Chapter Chapter, Story Story)>)new List<(Chapter, Story)>(), 0);
            });

        await _handler.Handle(new GetReviewedChaptersQuery { Status = status }, CancellationToken.None);

        captured.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmpty_When_TypeIsNotChapter()
    {
        var result = await _handler.Handle(
            new GetReviewedChaptersQuery { Status = "Approved", Type = "Story" },
            CancellationToken.None);

        result.TotalCount.Should().Be(0);
        await _chapterRepository.DidNotReceive().GetReviewedAsync(
            Arg.Any<ChapterReviewActionType>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapRows()
    {
        var chapter = new Chapter { Id = 1, PublicId = Guid.NewGuid(), Title = "Ch", Status = ChapterStatus.Published };
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), Title = "S", Slug = "s" };
        _chapterRepository.GetReviewedAsync(ChapterReviewActionType.Approved, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<(Chapter Chapter, Story Story)>)new List<(Chapter, Story)> { (chapter, story) }, 1));

        var result = await _handler.Handle(new GetReviewedChaptersQuery { Status = "Approved" }, CancellationToken.None);

        result.Items.Should().ContainSingle(x => x.ChapterId == chapter.PublicId);
    }
}
