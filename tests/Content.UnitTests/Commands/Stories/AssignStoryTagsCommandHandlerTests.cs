using Content.Application.Commands.Stories.AssignStoryTags;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class AssignStoryTagsCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<AssignStoryTagsCommandHandler> _logger =
        Substitute.For<ILogger<AssignStoryTagsCommandHandler>>();

    private readonly AssignStoryTagsCommandHandler _handler;

    public AssignStoryTagsCommandHandlerTests()
    {
        _handler = new AssignStoryTagsCommandHandler(_storyRepository, _tagRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ReplaceStoryTags_When_OwnerAssignsNewTags()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var tag = new Tag { Id = 1, Name = "Isekai", Slug = "isekai", UsageCount = 0 };
        _tagRepository.GetOrCreateBySlugAsync(
                Arg.Any<IReadOnlyCollection<(string Name, string Slug)>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tag> { tag });

        var command = new AssignStoryTagsCommand { StoryId = story.PublicId, Tags = new List<string> { "Isekai" } };

        var result = await _handler.Handle(command, CancellationToken.None);

        story.Tags.Should().ContainSingle(st => st.TagId == tag.Id);
        tag.UsageCount.Should().Be(1);
        result.Tags.Should().ContainSingle(t => t == "Isekai");
        _storyRepository.Received(1).Update(story);
    }
}
