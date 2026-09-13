using Content.Application.Dtos;
using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Stories.GetStories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetStoriesQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();

    private readonly GetStoriesQueryHandler _handler;

    public GetStoriesQueryHandlerTests()
    {
        _handler = new GetStoriesQueryHandler(_storyRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedSummaries_When_StoriesExist()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Title = "A Story",
            Slug = "a-story",
            Status = StoryStatus.Ongoing
        };

        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((new List<Story> { story }, 1));

        var query = new GetStoriesQuery { PageNumber = 1, PageSize = 10 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().ContainSingle(s => s.Id == story.PublicId);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_Should_ClampPageSize_When_RequestedPageSizeExceedsMax()
    {
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((new List<Story>(), 0));

        var query = new GetStoriesQuery { PageNumber = 1, PageSize = 1000 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Handle_Should_IgnoreDraftStatusFilter_When_RequestedExplicitly()
    {
        StorySearchCriteria capturedCriteria = null;
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                capturedCriteria = ci.Arg<StorySearchCriteria>();
                return (new List<Story>(), 0);
            });

        var query = new GetStoriesQuery { Status = "Draft" };

        await _handler.Handle(query, CancellationToken.None);

        capturedCriteria.Should().NotBeNull();
        capturedCriteria!.Status.Should().BeNull();
    }

    [Theory]
    [InlineData("long", ChapterLengthFilter.Long)]
    [InlineData("Short", ChapterLengthFilter.Short)]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public async Task Handle_Should_ParseChapterLength_When_RequestedInAnyCasing(string input, ChapterLengthFilter? expected)
    {
        StorySearchCriteria capturedCriteria = null;
        _storyRepository.SearchPublishedAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                capturedCriteria = ci.Arg<StorySearchCriteria>();
                return (new List<Story>(), 0);
            });

        var query = new GetStoriesQuery { Length = input };

        await _handler.Handle(query, CancellationToken.None);

        capturedCriteria.Should().NotBeNull();
        capturedCriteria!.ChapterLength.Should().Be(expected);
    }
}
