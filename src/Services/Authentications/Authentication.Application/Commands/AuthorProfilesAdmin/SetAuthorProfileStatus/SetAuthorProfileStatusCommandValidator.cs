namespace Authentication.Application.Commands.AuthorProfilesAdmin.SetAuthorProfileStatus;

/// <summary>Validation rules for <see cref="SetAuthorProfileStatusCommand"/>.</summary>
public sealed class SetAuthorProfileStatusCommandValidator : AbstractValidator<SetAuthorProfileStatusCommand>
{
    /// <summary>Requires a positive profile id and a recognized status name.</summary>
    public SetAuthorProfileStatusCommandValidator()
    {
        RuleFor(x => x.AuthorProfileId).GreaterThan(0);

        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<AuthorProfileStatus>(status, ignoreCase: true, out _))
            .WithMessage(ApplicationErrorConstants.InvalidAuthorProfileStatus);
    }
}
