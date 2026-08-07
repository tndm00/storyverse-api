namespace Be.StoryVerse.Core.Interfaces.Commands;

/// <summary>
/// Marker interface for a write use case dispatched through MediatR.
/// </summary>
/// <typeparam name="TResponse">Type returned after the command executes.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
