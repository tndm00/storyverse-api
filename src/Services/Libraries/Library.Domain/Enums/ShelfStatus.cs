namespace Library.Domain.Enums;

/// <summary>
/// How a reader has deliberately shelved a story in their personal library,
/// per the "Đọc &amp; thư viện" section of the domain spec. Distinct from
/// automatic reading history, which lives on
/// <see cref="Library.Domain.Entities.ReadingProgress"/> (spec rule 07).
/// </summary>
public enum ShelfStatus
{
    Reading,
    PlanToRead,
    Completed,
    Dropped
}
