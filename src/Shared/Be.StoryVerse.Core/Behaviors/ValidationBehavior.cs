namespace Be.StoryVerse.Core.Behaviors;

/// <summary>
/// MediatR pipeline step that runs every registered FluentValidation validator
/// for a request before its handler executes, per api-guidelines.md section 14
/// and code-standard.md section 33. Aggregated failures become a single
/// <see cref="BadRequestException"/> so the global error envelope stays consistent.
/// </summary>
/// <remarks>
/// Lives in Be.StoryVerse.Core so every service shares one implementation. A
/// service activates it by registering
/// <c>IPipelineBehavior&lt;,&gt; -> ValidationBehavior&lt;,&gt;</c> in its
/// Application DI setup; without that registration, validators never run.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => failure.ErrorMessage)
            .Distinct()
            .ToArray();

        if (failures.Length > 0)
        {
            throw new BadRequestException(string.Join(" ", failures));
        }

        return await next();
    }
}
