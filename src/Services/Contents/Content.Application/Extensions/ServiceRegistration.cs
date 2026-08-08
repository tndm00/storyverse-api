namespace Content.Application.Extensions;

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

        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        return services;
    }
}
