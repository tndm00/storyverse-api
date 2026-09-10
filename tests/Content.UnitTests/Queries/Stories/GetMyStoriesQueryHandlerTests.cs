using Be.StoryVerse.Core.Exceptions;
using Content.Application.Dtos;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Stories.GetMyStories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetMyStoriesQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly GetMyStoriesQueryHandler _handler;

    public GetMyStoriesQueryHandlerTests()
    {
        _handler = new GetMyStoriesQueryHandler(_storyRepository, _authorContext);
    }

    [Fact]
    public async Task Handle_Should_ScopeToCallerAuthorProfile_And_IncludeDrafts()
    {
        _authorContext.GetAuthorProfileId().Returns(42L);
        StorySearchCriteria captured = null;
        _storyRepository.SearchAllAsync(Arg.Any<StorySearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                captured = ci.Arg<StorySearchCriteria>();
                var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 42, Status = StoryStatus.Draft };
                return ((IReadOnlyList<Story>)new List<Story> { story }, 1);
            });

        var result = await _handler.Handle(new GetMyStoriesQuery(), CancellationToken.None);

        captured!.AuthorProfileId.Should().Be(42L);
        result.Items.Should().ContainSingle(s => s.Status == "Draft");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_CallerHasNoAuthorProfile()
    {
        _authorContext.GetAuthorProfileId().Returns(_ => throw new ForbiddenException("no profile"));

        Func<Task> act = () => _handler.Handle(new GetMyStoriesQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
