using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Library.Application.Commands.LibraryEntries.RemoveLibraryEntry;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Commands.LibraryEntries;

public class RemoveLibraryEntryCommandHandlerTests
{
    private readonly ILibraryEntryRepository _libraryEntryRepository = Substitute.For<ILibraryEntryRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<RemoveLibraryEntryCommandHandler> _logger =
        Substitute.For<ILogger<RemoveLibraryEntryCommandHandler>>();

    private readonly RemoveLibraryEntryCommandHandler _handler;

    public RemoveLibraryEntryCommandHandlerTests()
    {
        _handler = new RemoveLibraryEntryCommandHandler(_libraryEntryRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EntryDoesNotExist()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns((LibraryEntry)null);

        var command = new RemoveLibraryEntryCommand { StoryId = storyId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _libraryEntryRepository.DidNotReceive().Remove(Arg.Any<LibraryEntry>());
    }

    [Fact]
    public async Task Handle_Should_RemoveEntryAndPersist_When_EntryExists()
    {
        var storyId = Guid.NewGuid();
        var entry = new LibraryEntry { Id = 1, UserId = 1L, StoryId = storyId };
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(entry);

        var command = new RemoveLibraryEntryCommand { StoryId = storyId };

        await _handler.Handle(command, CancellationToken.None);

        _libraryEntryRepository.Received(1).Remove(entry);
        await _libraryEntryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
