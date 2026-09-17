namespace Moderation.Application.Extensions;

/// <summary>
/// Registers Application-layer services: MediatR handlers + validation pipeline,
/// FluentValidation validators, and Mapster mappings, per
/// codebase-architecture-flow.md section 8 (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    /// <summary>Registers MediatR, FluentValidation, the validation pipeline behavior, Mapster mappings, and the report enricher.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Runs request validators before their handler; without this the
        // registered validators would never execute.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        TypeAdapterConfig.GlobalSettings.Scan(assembly);

        // Composes the Auth/Content/Community lookup clients to enrich report DTOs.
        services.AddScoped<Services.IReportEnricher, Services.ReportEnricher>();

        return services;
    }
}
