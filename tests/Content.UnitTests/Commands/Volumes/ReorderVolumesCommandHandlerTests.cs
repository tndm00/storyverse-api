using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Volumes.ReorderVolumes;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Volumes;

public class ReorderVolumesCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<ReorderVolumesCommandHandler> _logger =
        Substitute.For<ILogger<ReorderVolumesCommandHandler>>();

    private readonly ReorderVolumesCommandHandler _handler;

    private readonly Guid _storyId = Guid.NewGuid();
    private readonly Volume _v1 = new() { Id = 1, StoryId = 5, PublicId = Guid.NewGuid(), Title = "A", OrderIndex = 1 };
    private readonly Volume _v2 = new() { Id = 2, StoryId = 5, PublicId = Guid.NewGuid(), Title = "B", OrderIndex = 2 };
    private readonly Volume _v3 = new() { Id = 3, StoryId = 5, PublicId = Guid.NewGuid(), Title = "C", OrderIndex = 3 };

    public ReorderVolumesCommandHandlerTests()
    {
        _handler = new ReorderVolumesCommandHandler(_storyRepository, _volumeRepository, _authorContext, _logger);

        _storyRepository.GetByPublicIdAsync(_storyId, Arg.Any<CancellationToken>())
            .Returns(new Story { Id = 5, PublicId = _storyId, AuthorProfileId = 10 });
        _volumeRepository.GetByStoryTrackedAsync(5, Arg.Any<CancellationToken>())
            .Returns(new[] { _v1, _v2, _v3 });
        _authorContext.GetAuthorProfileId().Returns(10L);
    }

    [Fact]
    public async Task Handle_Should_SetOrderIndexByPosition_And_ReturnRequestedOrder()
    {
        var command = new ReorderVolumesCommand
        {
            StoryId = _storyId,
            OrderedVolumeIds = new[] { _v3.PublicId, _v1.PublicId, _v2.PublicId }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        _v3.OrderIndex.Should().Be(1);
        _v1.OrderIndex.Should().Be(2);
        _v2.OrderIndex.Should().Be(3);
        result.Select(x => x.Id).Should().ContainInOrder(_v3.PublicId, _v1.PublicId, _v2.PublicId);
        await _volumeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_When_IdSetDoesNotMatchStoryVolumes()
    {
        var command = new ReorderVolumesCommand
        {
            StoryId = _storyId,
            OrderedVolumeIds = new[] { _v1.PublicId, _v2.PublicId }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_CallerIsNotOwnerAndNotModerator()
    {
        _authorContext.GetAuthorProfileId().Returns(99L);

        var command = new ReorderVolumesCommand
        {
            StoryId = _storyId,
            OrderedVolumeIds = new[] { _v1.PublicId, _v2.PublicId, _v3.PublicId }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_AllowReorder_When_CallerHasContentModerate()
    {
        _authorContext.GetAuthorProfileId().Returns(_ => throw new ForbiddenException("no author profile"));
        _authorContext.HasPermission(Arg.Any<string>()).Returns(true);

        var command = new ReorderVolumesCommand
        {
            StoryId = _storyId,
            OrderedVolumeIds = new[] { _v2.PublicId, _v3.PublicId, _v1.PublicId }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().HaveCount(3);
        _v2.OrderIndex.Should().Be(1);
    }
}
