namespace Library.Application.Commands.LibraryEntries.RemoveLibraryEntry;

public sealed class RemoveLibraryEntryCommandHandler : ICommandHandler<RemoveLibraryEntryCommand, Unit>
{
    private readonly ILibraryEntryRepository _libraryEntryRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<RemoveLibraryEntryCommandHandler> _logger;

    public RemoveLibraryEntryCommandHandler(
        ILibraryEntryRepository libraryEntryRepository,
        ICurrentUserContext currentUser,
        ILogger<RemoveLibraryEntryCommandHandler> logger)
    {
        _libraryEntryRepository = libraryEntryRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveLibraryEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var entry = await _libraryEntryRepository.GetAsync(userId, request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.LibraryEntryNotFound);

        _libraryEntryRepository.Remove(entry);
        await _libraryEntryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.LibraryEntryRemoved, userId, request.StoryId);

        // Integration point: publish a "StoryUnfollowed" event so the Content service
        // can decrement Story.FollowCount and the Notification service can unsubscribe
        // this reader. No event bus implementation exists yet (Phase 1); the Library
        // service must never write the Content database directly.

        return Unit.Value;
    }
}
