namespace Be.StoryVerse.Core.Interfaces.Repositories;

/// <summary>
/// Generic persistence boundary for aggregate-style entities. Service-specific
/// repositories (for example <c>IUserRepository</c>) should extend this only
/// when the generic operations genuinely apply; otherwise define business-readable
/// methods directly on the specific repository interface, per code-standard.md
/// section 28.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
