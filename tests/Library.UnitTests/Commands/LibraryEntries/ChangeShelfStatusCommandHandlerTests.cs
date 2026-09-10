using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Library.Application.Commands.LibraryEntries.ChangeShelfStatus;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Commands.LibraryEntries;

public class ChangeShelfStatusCommandHandlerTests
{
    private readonly ILibraryEntryRepository _libraryEntryRepository = Substitute.For<ILibraryEntryRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<ChangeShelfStatusCommandHandler> _logger =
        Substitute.For<ILogger<ChangeShelfStatusCommandHandler>>();

    private readonly ChangeShelfStatusCommandHandler _handler;

    public ChangeShelfStatusCommandHandlerTests()
    {
        _handler = new ChangeShelfStatusCommandHandler(_libraryEntryRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EntryDoesNotExist()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns((LibraryEntry)null);

        var command = new ChangeShelfStatusCommand { StoryId = storyId, ShelfStatus = "Completed" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _libraryEntryRepository.DidNotReceive().Update(Arg.Any<LibraryEntry>());
    }

    [Fact]
    public async Task Handle_Should_UpdateShelfAndPersist_When_ShelfChanges()
    {
        var storyId = Guid.NewGuid();
        var entry = new LibraryEntry { Id = 1, UserId = 1L, StoryId = storyId, ShelfStatus = ShelfStatus.Reading };
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(entry);

        var command = new ChangeShelfStatusCommand { StoryId = storyId, ShelfStatus = "Completed" };

        var result = await _handler.Handle(command, CancellationToken.None);

        entry.ShelfStatus.Should().Be(ShelfStatus.Completed);
        entry.UpdatedAt.Should().NotBeNull();
        result.ShelfStatus.Should().Be(ShelfStatus.Completed.ToString());
        _libraryEntryRepository.Received(1).Update(entry);
        await _libraryEntryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotPersist_When_ShelfIsUnchanged()
    {
        var storyId = Guid.NewGuid();
        var entry = new LibraryEntry { Id = 1, UserId = 1L, StoryId = storyId, ShelfStatus = ShelfStatus.Reading };
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(entry);

        var command = new ChangeShelfStatusCommand { StoryId = storyId, ShelfStatus = "Reading" };

        await _handler.Handle(command, CancellationToken.None);

        _libraryEntryRepository.DidNotReceive().Update(Arg.Any<LibraryEntry>());
        await _libraryEntryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
