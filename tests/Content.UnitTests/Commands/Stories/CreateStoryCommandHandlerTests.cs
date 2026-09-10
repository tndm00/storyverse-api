using Content.Application.Commands.Stories.CreateStory;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class CreateStoryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<CreateStoryCommandHandler> _logger =
        Substitute.For<ILogger<CreateStoryCommandHandler>>();

    private readonly CreateStoryCommandHandler _handler;

    public CreateStoryCommandHandlerTests()
    {
        _handler = new CreateStoryCommandHandler(_storyRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_CreateDraftStory_When_RequestIsValid()
    {
        _authorContext.GetAuthorProfileId().Returns(10L);
        _storyRepository.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateStoryCommand
        {
            Title = "My New Story",
            Description = "A description",
            ContentType = StoryContentType.Original,
            AgeRating = AgeRating.General
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("My New Story");
        result.Slug.Should().Be("my-new-story");
        result.Status.Should().Be(StoryStatus.Draft.ToString());
        await _storyRepository.Received(1).AddAsync(
            Arg.Is<Content.Domain.Entities.Story>(s =>
                s.AuthorProfileId == 10L && s.Status == StoryStatus.Draft),
            Arg.Any<CancellationToken>());
        await _storyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
