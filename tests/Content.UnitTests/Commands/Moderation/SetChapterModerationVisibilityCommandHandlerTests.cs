using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Moderation.SetChapterModerationVisibility;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Moderation;

public class SetChapterModerationVisibilityCommandHandlerTests
{
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly SetChapterModerationVisibilityCommandHandler _handler;

    public SetChapterModerationVisibilityCommandHandlerTests()
    {
        _handler = new SetChapterModerationVisibilityCommandHandler(
            _chapterRepository, Substitute.For<ILogger<SetChapterModerationVisibilityCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_ChapterMissing()
    {
        _chapterRepository.GetByPublicIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Chapter)null);

        Func<Task> act = () => _handler.Handle(
            new SetChapterModerationVisibilityCommand { ChapterId = Guid.NewGuid(), Hidden = true }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_SetRemovedAndStoreReason_When_Hiding()
    {
        var id = Guid.NewGuid();
        var chapter = new Chapter { PublicId = id, Status = ChapterStatus.Published };
        _chapterRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(chapter);

        var result = await _handler.Handle(
            new SetChapterModerationVisibilityCommand { ChapterId = id, Hidden = true, Reason = "copyright" },
            CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Removed);
        chapter.RejectionReason.Should().Be("copyright");
        result.Status.Should().Be("Removed");
        await _chapterRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RestoreToPublished_When_UnhidingRemovedChapter()
    {
        var id = Guid.NewGuid();
        var chapter = new Chapter { PublicId = id, Status = ChapterStatus.Removed };
        _chapterRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(chapter);

        var result = await _handler.Handle(
            new SetChapterModerationVisibilityCommand { ChapterId = id, Hidden = false }, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Published);
        result.Status.Should().Be("Published");
    }

    [Fact]
    public async Task Handle_Should_BeNoOp_When_HidingAlreadyRemovedChapter()
    {
        var id = Guid.NewGuid();
        var chapter = new Chapter { PublicId = id, Status = ChapterStatus.Removed };
        _chapterRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(chapter);

        await _handler.Handle(
            new SetChapterModerationVisibilityCommand { ChapterId = id, Hidden = true }, CancellationToken.None);

        await _chapterRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
