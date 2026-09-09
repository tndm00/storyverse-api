namespace Content.Domain.Entities;

/// <summary>
/// Free-form label that supplements <see cref="Genre"/> (motif, setting, trope).
/// Open list: created on demand when an author types a new one, matched by
/// <see cref="Slug"/> to avoid case/whitespace duplicates. Never replaces Genre
/// for discovery (product-workflow-context.md section 4).
/// </summary>
public sealed class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    /// <summary>Denormalized count of stories using this tag, for "popular tags" listings.</summary>
    public int UsageCount { get; set; }
}
