namespace Content.Domain.Enums;

/// <summary>
/// Publication lifecycle of a <see cref="Content.Domain.Entities.Story"/>, per
/// product-workflow-context.md section 7. A story reaches <see cref="Ongoing"/>
/// only once it has at least one published chapter.
/// </summary>
public enum StoryStatus
{
    Draft,
    Ongoing,
    Completed,
    Hiatus,
    Dropped,

    /// <summary>
    /// Taken down by a moderator (Moderation service Hide/Remove decision). The
    /// story and its chapters are withheld from public discovery/reading until a
    /// moderator restores it. Set only through the internal moderation-visibility
    /// endpoint, never by the author.
    /// </summary>
    Removed
}
