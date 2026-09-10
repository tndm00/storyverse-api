using FluentAssertions;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Application.Queries.ReadingProgresses.GetContinueReading;
using Library.Domain.Entities;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Queries.ReadingProgresses;

public class GetContinueReadingQueryHandlerTests
{
    private readonly IReadingProgressRepository _readingProgressRepository =
        Substitute.For<IReadingProgressRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();

    private readonly GetContinueReadingQueryHandler _handler;

    public GetContinueReadingQueryHandlerTests()
    {
        _handler = new GetContinueReadingQueryHandler(_readingProgressRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyPage_When_NoProgressRecorded()
    {
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetRecentAsync(1L, 1, 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<ReadingProgress>(), 0));

        var query = new GetContinueReadingQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_MapRecentProgressRows_Into_PagedResponse()
    {
        var storyId = Guid.NewGuid();
        var progress = new ReadingProgress { Id = 1, UserId = 1L, StoryId = storyId };
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetRecentAsync(1L, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new[] { progress }, 1));

        var query = new GetContinueReadingQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Single().StoryId.Should().Be(storyId);
    }

    [Fact]
    public async Task Handle_Should_ClampPageSize_When_RequestedPageSizeExceedsMax()
    {
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetRecentAsync(1L, 1, 50, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<ReadingProgress>(), 0));

        var query = new GetContinueReadingQuery { PageSize = 500 };

        await _handler.Handle(query, CancellationToken.None);

        await _readingProgressRepository.Received(1).GetRecentAsync(1L, 1, 50, Arg.Any<CancellationToken>());
    }
}
