using Content.Application.Dtos;
using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Stories.GetAdminStories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetAdminStoriesQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly GetAdminStoriesQueryHandler _handler;

    public GetAdminStoriesQueryHandlerTests()
    {
        _handler = new GetAdminStoriesQueryHandler(_storyRepository);
    }

    [Fact]
    public async Task Handle_Should_QuerySearchAllAsync_And_KeepDraftStatusFilter()
    {
        StorySearchCriteria captured = null;
        _storyRepository.SearchAllAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<StorySearchCriteria>();
                var story = new Story { Id = 1, PublicId = Guid.NewGuid(), Status = StoryStatus.Draft };
                return ((IReadOnlyList<Story>)new List<Story> { story }, 1);
            });

        var result = await _handler.Handle(
            new GetAdminStoriesQuery { Status = "Draft", Keyword = " dragon ", PageSize = 10 },
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(StoryStatus.Draft);
        captured.Keyword.Should().Be("dragon");
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(s => s.Status == "Draft");
    }

    [Fact]
    public async Task Handle_Should_ClampPageSize()
    {
        _storyRepository.SearchAllAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<Story>)new List<Story>(), 0));

        var result = await _handler.Handle(new GetAdminStoriesQuery { PageSize = 1000 }, CancellationToken.None);

        result.PageSize.Should().Be(50);
    }
}
