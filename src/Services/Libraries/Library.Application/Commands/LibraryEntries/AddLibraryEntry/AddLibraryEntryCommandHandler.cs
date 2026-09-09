namespace Library.Application.Commands.LibraryEntries.AddLibraryEntry;

public sealed class AddLibraryEntryCommandHandler : ICommandHandler<AddLibraryEntryCommand, LibraryEntryResponseDto>
{
    private readonly ILibraryEntryRepository _libraryEntryRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<AddLibraryEntryCommandHandler> _logger;

    public AddLibraryEntryCommandHandler(
        ILibraryEntryRepository libraryEntryRepository,
        ICurrentUserContext currentUser,
        ILogger<AddLibraryEntryCommandHandler> logger)
    {
        _libraryEntryRepository = libraryEntryRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<LibraryEntryResponseDto> Handle(AddLibraryEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        if (await _libraryEntryRepository.ExistsAsync(userId, request.StoryId, cancellationToken))
        {
            throw new ConflictException(ApplicationErrorConstants.LibraryEntryAlreadyExists);
        }

        var shelfStatus = ShelfStatusParser.ParseOrDefault(request.ShelfStatus, ShelfStatus.Reading);
        var now = DateTime.UtcNow;

        var entry = new LibraryEntry
        {
            UserId = userId,
            StoryId = request.StoryId,
            ShelfStatus = shelfStatus,
            AddedAt = now
        };

        await _libraryEntryRepository.AddAsync(entry, cancellationToken);
        await _libraryEntryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.LibraryEntryAdded, userId, request.StoryId, shelfStatus);

        // Integration point: publish a "StoryFollowed" event so the Content service
        // can increment Story.FollowCount and the Notification service can subscribe
        // this reader to new-chapter alerts. No event bus implementation exists yet
        // (Phase 1); the Library service must never write the Content database directly.

        return LibraryDtoMapper.ToDto(entry);
    }
}
