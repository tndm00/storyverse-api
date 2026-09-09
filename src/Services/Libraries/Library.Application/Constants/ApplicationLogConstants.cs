namespace Library.Application.Constants;

/// <summary>
/// Structured-logging message templates for the Library service. No secrets and
/// no personal data beyond ids, per code-standard.md section 12.
/// </summary>
public static class ApplicationLogConstants
{
    public const string LibraryEntryAdded = "User {UserId} added story {StoryId} to their library with shelf status {ShelfStatus}.";
    public const string LibraryEntryShelfChanged = "User {UserId} changed shelf status of story {StoryId} to {ShelfStatus}.";
    public const string LibraryEntryRemoved = "User {UserId} removed story {StoryId} from their library.";

    public const string ReadingProgressUpserted = "User {UserId} reading progress for story {StoryId} advanced to chapter {ChapterId}.";
}
