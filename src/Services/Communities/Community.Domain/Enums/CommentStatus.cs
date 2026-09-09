namespace Community.Domain.Enums;

/// <summary>
/// Lifecycle of a <see cref="Community.Domain.Entities.Comment"/>, per the domain
/// analysis section 4 (Tương tác cộng đồng). <see cref="Deleted"/> is a soft delete
/// by the author; <see cref="Hidden"/> is a moderator action.
/// </summary>
public enum CommentStatus
{
    Visible,
    Hidden,
    Deleted
}
