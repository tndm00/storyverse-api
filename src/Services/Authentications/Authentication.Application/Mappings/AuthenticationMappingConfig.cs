namespace Authentication.Application.Mappings;

/// <summary>
/// Explicit Mapster mapping configuration for the Authentication service.
/// Entities are never returned directly as API DTOs, per code-standard.md
/// section 19; these mappings are the only place that translates between them.
/// </summary>
public sealed class AuthenticationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, RegisterResponseDto>()
            .Map(dest => dest.UserId, src => src.Id);

        config.NewConfig<User, CurrentUserResponseDto>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.LastLoginAt, src => src.LastLoginAt)
            // Role names are resolved separately (User.Roles holds join rows, not
            // strings); the query handler sets this after mapping.
            .Ignore(dest => dest.Roles);
    }
}
