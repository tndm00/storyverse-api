namespace Content.Domain.Entities;

/// <summary>
/// Primary classification of a story. Closed list, managed only by Admin. Never
/// hard-deleted while referenced by a story — hidden via <see cref="IsActive"/>
/// instead (rules section 10.5). Distinct from the open-ended <see cref="Tag"/>.
/// </summary>
public sealed class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>When false the genre is hidden from new story selections but kept for existing references.</summary>
    public bool IsActive { get; set; } = true;
}
