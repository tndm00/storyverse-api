namespace Authentication.Application.Extensions;

/// <summary>
/// Registers Application-layer services: MediatR handlers, FluentValidation
/// validators, and Mapster mappings, per codebase-architecture-flow.md section 8
/// (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Registers IRegister implementations such as AuthenticationMappingConfig
        // against the global Mapster config; handlers call .Adapt<T>() directly.
        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
