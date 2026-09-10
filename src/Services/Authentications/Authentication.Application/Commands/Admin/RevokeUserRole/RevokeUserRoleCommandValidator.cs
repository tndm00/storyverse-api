namespace Authentication.Application.Commands.Admin.RevokeUserRole;

public sealed class RevokeUserRoleCommandValidator : AbstractValidator<RevokeUserRoleCommand>
{
    public RevokeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Role)
            .Must(role => RoleNameParser.TryParse(role, out _))
            .WithMessage(ApplicationErrorConstants.InvalidRoleName);
    }
}
