namespace Be.StoryVerse.Core.Interfaces.Commands;

/// <summary>
/// Handles a single <see cref="ICommand{TResponse}"/>.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
