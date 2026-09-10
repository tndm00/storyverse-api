using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Application.Queries.ReadingProgresses.GetStoryProgress;
using Library.Domain.Entities;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Queries.ReadingProgresses;

public class GetStoryProgressQueryHandlerTests
{
    private readonly IReadingProgressRepository _readingProgressRepository =
        Substitute.For<IReadingProgressRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();

    private readonly GetStoryProgressQueryHandler _handler;

    public GetStoryProgressQueryHandlerTests()
    {
        _handler = new GetStoryProgressQueryHandler(_readingProgressRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_NoProgressRecorded()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>())
            .Returns((ReadingProgress)null);

        var query = new GetStoryProgressQuery { StoryId = storyId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnMappedProgress_When_ProgressExists()
    {
        var storyId = Guid.NewGuid();
        var chapterId = Guid.NewGuid();
        var progress = new ReadingProgress { Id = 1, UserId = 1L, StoryId = storyId, LastChapterId = chapterId };
        _currentUser.GetUserId().Returns(1L);
        _readingProgressRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(progress);

        var query = new GetStoryProgressQuery { StoryId = storyId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.StoryId.Should().Be(storyId);
        result.LastChapterId.Should().Be(chapterId);
    }
}
