namespace Content.Application.Extensions;

/// <summary>
/// Registers Application-layer services: MediatR handlers + validation pipeline,
/// FluentValidation validators, and Mapster mappings, per
/// codebase-architecture-flow.md section 8 (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    /// <summary>Wires up MediatR handlers, FluentValidation validators, the validation pipeline behavior, and Mapster mappings for this assembly.</summary>
    /// <param name="services">The DI container to register Application-layer services into.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Discover and register all MediatR request handlers and FluentValidation validators in this assembly.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Runs request validators before their handler; without this the
        // registered validators would never execute.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Scan the assembly for Mapster TypeAdapterConfig registrations (e.g. ContentDtoMapper).
        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
