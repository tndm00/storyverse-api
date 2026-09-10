using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.ReorderChapters;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class ReorderChaptersCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<ReorderChaptersCommandHandler> _logger =
        Substitute.For<ILogger<ReorderChaptersCommandHandler>>();

    private readonly ReorderChaptersCommandHandler _handler;

    private readonly Story _story = new() { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
    private readonly Chapter _c1 = new() { Id = 1, StoryId = 5, PublicId = Guid.NewGuid(), OrderIndex = 1 };
    private readonly Chapter _c2 = new() { Id = 2, StoryId = 5, PublicId = Guid.NewGuid(), OrderIndex = 2 };

    public ReorderChaptersCommandHandlerTests()
    {
        _handler = new ReorderChaptersCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _logger);

        _authorContext.GetAuthorProfileId().Returns(10L);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(_story);
        _storyRepository.GetByPublicIdAsync(_story.PublicId, Arg.Any<CancellationToken>()).Returns(_story);
    }

    [Fact]
    public async Task Handle_Should_ReorderVolumeChapters_And_ReturnVolumeId()
    {
        var volume = new Volume { Id = 3, StoryId = 5, PublicId = Guid.NewGuid() };
        _volumeRepository.GetByPublicIdAsync(volume.PublicId, Arg.Any<CancellationToken>()).Returns(volume);
        _chapterRepository.GetByVolumeTrackedAsync(3, Arg.Any<CancellationToken>()).Returns(new[] { _c1, _c2 });

        var command = new ReorderChaptersCommand
        {
            VolumeId = volume.PublicId,
            OrderedChapterIds = new[] { _c2.PublicId, _c1.PublicId }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        _c2.OrderIndex.Should().Be(1);
        _c1.OrderIndex.Should().Be(2);
        result.Select(x => x.Id).Should().ContainInOrder(_c2.PublicId, _c1.PublicId);
        result.Should().OnlyContain(x => x.VolumeId == volume.PublicId);
        await _chapterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReorderStoryChaptersWithoutVolume()
    {
        _chapterRepository.GetStoryChaptersWithoutVolumeTrackedAsync(5, Arg.Any<CancellationToken>())
            .Returns(new[] { _c1, _c2 });

        var command = new ReorderChaptersCommand
        {
            StoryId = _story.PublicId,
            OrderedChapterIds = new[] { _c2.PublicId, _c1.PublicId }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        _c2.OrderIndex.Should().Be(1);
        result.Should().OnlyContain(x => x.VolumeId == null);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_ChapterIdSetIncomplete()
    {
        _chapterRepository.GetStoryChaptersWithoutVolumeTrackedAsync(5, Arg.Any<CancellationToken>())
            .Returns(new[] { _c1, _c2 });

        var command = new ReorderChaptersCommand
        {
            StoryId = _story.PublicId,
            OrderedChapterIds = new[] { _c1.PublicId }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_VolumeNotFound()
    {
        var missing = Guid.NewGuid();
        _volumeRepository.GetByPublicIdAsync(missing, Arg.Any<CancellationToken>()).Returns((Volume)null);

        var command = new ReorderChaptersCommand
        {
            VolumeId = missing,
            OrderedChapterIds = new[] { _c1.PublicId }
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
