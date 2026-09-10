using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Volumes.GetStoryVolumes;
using Content.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Volumes;

public class GetStoryVolumesQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();

    private readonly GetStoryVolumesQueryHandler _handler;

    public GetStoryVolumesQueryHandlerTests()
    {
        _handler = new GetStoryVolumesQueryHandler(_storyRepository, _volumeRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnVolumesOrderedByOrderIndex_When_StoryExists()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid() };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);

        var volumeTwo = new Volume { Id = 2, StoryId = 1, PublicId = Guid.NewGuid(), Title = "Volume 2", OrderIndex = 2 };
        var volumeOne = new Volume { Id = 1, StoryId = 1, PublicId = Guid.NewGuid(), Title = "Volume 1", OrderIndex = 1 };
        _volumeRepository.GetByStoryAsync(1, Arg.Any<CancellationToken>())
            .Returns(new List<Volume> { volumeTwo, volumeOne });

        var query = new GetStoryVolumesQuery { StoryId = story.PublicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Volume 1");
        result[1].Title.Should().Be("Volume 2");
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_StoryDoesNotExist()
    {
        var storyPublicId = Guid.NewGuid();
        _storyRepository.GetByPublicIdAsync(storyPublicId, Arg.Any<CancellationToken>()).Returns((Story)null);

        var query = new GetStoryVolumesQuery { StoryId = storyPublicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
