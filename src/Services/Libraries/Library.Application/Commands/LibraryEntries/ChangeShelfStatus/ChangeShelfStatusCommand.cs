namespace Library.Application.Commands.LibraryEntries.ChangeShelfStatus;

/// <summary>Moves one of the caller's library entries to a different shelf.</summary>
public sealed class ChangeShelfStatusCommand : ICommand<LibraryEntryResponseDto>
{
    public Guid StoryId { get; init; }

    public string ShelfStatus { get; init; }
}
