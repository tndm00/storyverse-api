namespace Be.StoryVerse.Core.Interfaces.Repositories;

/// <summary>
/// Commits pending changes tracked across repositories for a single DbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
