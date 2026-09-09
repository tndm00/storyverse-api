namespace Content.Domain.Enums;

/// <summary>
/// Access model for a chapter. Phase 1 is free-only: every chapter is
/// <see cref="Free"/>. <see cref="Locked"/> and the accompanying price field
/// exist for Phase 2 (paid unlock) and must never gate content in Phase 1,
/// per product-workflow-context.md section 4 ("Free vs Locked Chapter").
/// </summary>
public enum ChapterAccessType
{
    Free,
    Locked
}
