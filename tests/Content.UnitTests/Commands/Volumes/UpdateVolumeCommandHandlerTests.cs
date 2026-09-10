using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Volumes.UpdateVolume;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Volumes;

public class UpdateVolumeCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<UpdateVolumeCommandHandler> _logger =
        Substitute.For<ILogger<UpdateVolumeCommandHandler>>();

    private readonly UpdateVolumeCommandHandler _handler;

    public UpdateVolumeCommandHandlerTests()
    {
        _handler = new UpdateVolumeCommandHandler(_storyRepository, _volumeRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_UpdateVolume_When_CallerOwnsStory()
    {
        var volumePublicId = Guid.NewGuid();
        var volume = new Volume { Id = 1, StoryId = 5, PublicId = volumePublicId, Title = "Old", OrderIndex = 1 };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _volumeRepository.GetByPublicIdAsync(volumePublicId, Arg.Any<CancellationToken>()).Returns(volume);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new UpdateVolumeCommand { VolumeId = volumePublicId, Title = "New Title", OrderIndex = 2 };

        var result = await _handler.Handle(command, CancellationToken.None);

        volume.Title.Should().Be("New Title");
        volume.OrderIndex.Should().Be(2);
        result.Title.Should().Be("New Title");
        _volumeRepository.Received(1).Update(volume);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_VolumeDoesNotExist()
    {
        var volumePublicId = Guid.NewGuid();
        _volumeRepository.GetByPublicIdAsync(volumePublicId, Arg.Any<CancellationToken>()).Returns((Volume)null);

        var command = new UpdateVolumeCommand { VolumeId = volumePublicId, Title = "New Title", OrderIndex = 2 };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
