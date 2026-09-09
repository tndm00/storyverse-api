namespace Library.Application.Commands.LibraryEntries.RemoveLibraryEntry;

/// <summary>Removes a story from the caller's library (an explicit unfollow).</summary>
public sealed class RemoveLibraryEntryCommand : ICommand<Unit>
{
    public Guid StoryId { get; init; }
}
