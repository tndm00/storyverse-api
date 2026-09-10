namespace Authentication.Application.Commands.Admin.GrantUserRole;

public sealed class GrantUserRoleCommandValidator : AbstractValidator<GrantUserRoleCommand>
{
    public GrantUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);

        RuleFor(x => x.Role)
            .Must(role => RoleNameParser.TryParse(role, out _))
            .WithMessage(ApplicationErrorConstants.InvalidRoleName);
    }
}
