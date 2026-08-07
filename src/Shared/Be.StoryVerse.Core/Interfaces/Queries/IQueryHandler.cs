namespace Be.StoryVerse.Core.Interfaces.Queries;

/// <summary>
/// Handles a single <see cref="IQuery{TResponse}"/>.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
