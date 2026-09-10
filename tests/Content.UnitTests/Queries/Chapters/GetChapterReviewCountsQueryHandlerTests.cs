using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Chapters.GetChapterReviewCounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetChapterReviewCountsQueryHandlerTests
{
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly GetChapterReviewCountsQueryHandler _handler;

    public GetChapterReviewCountsQueryHandlerTests()
    {
        _handler = new GetChapterReviewCountsQueryHandler(_chapterRepository);
    }

    [Fact]
    public async Task Handle_Should_ProjectAllFourCounters()
    {
        _chapterRepository.GetReviewCountsAsync(Arg.Any<CancellationToken>())
            .Returns((Pending: 4, InReview: 1, Approved: 7, Rejected: 3));

        var result = await _handler.Handle(new GetChapterReviewCountsQuery(), CancellationToken.None);

        result.Pending.Should().Be(4);
        result.InReview.Should().Be(1);
        result.Approved.Should().Be(7);
        result.Rejected.Should().Be(3);
    }
}
