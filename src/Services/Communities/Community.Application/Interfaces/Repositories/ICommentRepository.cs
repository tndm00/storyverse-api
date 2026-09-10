namespace Community.Application.Interfaces.Repositories;

/// <summary>Persistence boundary for chapter <see cref="Comment"/> threads.</summary>
public interface ICommentRepository
{
    Task<Comment> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    /// <summary>Public id -&gt; comment content for a set of comments, for internal cross-service lookups.</summary>
    Task<IReadOnlyList<Comment>> GetByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of visible comments for a chapter, newest first, with the total
    /// count of visible comments for that chapter.
    /// </summary>
    Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetVisibleByChapterAsync(
        Guid chapterId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);

    void Update(Comment comment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
