namespace Authentication.Application.Commands.Admin.GrantUserRole;

/// <summary>Validation rules for <see cref="GrantUserRoleCommand"/>.</summary>
public sealed class GrantUserRoleCommandValidator : AbstractValidator<GrantUserRoleCommand>
{
    /// <summary>Requires a positive user id and a recognized role name.</summary>
    public GrantUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Role)
            .Must(role => RoleNameParser.TryParse(role, out _))
            .WithMessage(ApplicationErrorConstants.InvalidRoleName);
    }
}
