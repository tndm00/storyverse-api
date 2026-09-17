namespace Authentication.Application.Commands.Admin.RevokeUserRole;

/// <summary>Validates <see cref="RevokeUserRoleCommand"/> input.</summary>
public sealed class RevokeUserRoleCommandValidator : AbstractValidator<RevokeUserRoleCommand>
{
    /// <summary>Defines validation rules for the target user id and role name.</summary>
    public RevokeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Role)
            .Must(role => RoleNameParser.TryParse(role, out _))
            .WithMessage(ApplicationErrorConstants.InvalidRoleName);
    }
}
