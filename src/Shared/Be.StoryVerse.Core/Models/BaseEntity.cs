namespace Be.StoryVerse.Core.Models;

/// <summary>
/// Common audit fields shared by domain entities across services, per
/// code-standard.md section 18 (Entity Rules).
/// </summary>
public abstract class BaseEntity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
