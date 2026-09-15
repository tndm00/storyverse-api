namespace Authentication.Application.Commands.AuthorProfilesAdmin.SetAuthorProfileStatus;

public sealed class SetAuthorProfileStatusCommandValidator : AbstractValidator<SetAuthorProfileStatusCommand>
{
    public SetAuthorProfileStatusCommandValidator()
    {
        RuleFor(x => x.AuthorProfileId).GreaterThan(0);

        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<AuthorProfileStatus>(status, ignoreCase: true, out _))
            .WithMessage(ApplicationErrorConstants.InvalidAuthorProfileStatus);
    }
}
