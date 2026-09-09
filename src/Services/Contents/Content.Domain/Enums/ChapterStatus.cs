namespace Content.Domain.Enums;

/// <summary>
/// Lifecycle of a single <see cref="Content.Domain.Entities.Chapter"/>, per
/// product-workflow-context.md section 7. A chapter has its own lifecycle
/// independent of its parent story's <see cref="StoryStatus"/>.
/// </summary>
public enum ChapterStatus
{
    Draft,
    Scheduled,
    Published,
    Removed
}
