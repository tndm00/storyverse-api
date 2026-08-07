namespace Be.StoryVerse.Core.Interfaces.Queries;

/// <summary>
/// Marker interface for a read use case dispatched through MediatR. Queries
/// must not change state, per api-guidelines.md section 9.
/// </summary>
/// <typeparam name="TResponse">Type returned after the query executes.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
