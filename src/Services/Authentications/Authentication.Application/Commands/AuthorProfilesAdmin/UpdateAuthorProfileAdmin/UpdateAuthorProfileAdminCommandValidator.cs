namespace Authentication.Application.Commands.AuthorProfilesAdmin.UpdateAuthorProfileAdmin;

/// <summary>Validation rules for <see cref="UpdateAuthorProfileAdminCommand"/>.</summary>
public sealed class UpdateAuthorProfileAdminCommandValidator : AbstractValidator<UpdateAuthorProfileAdminCommand>
{
    /// <summary>Requires a positive profile id and well-formed pen name / bio.</summary>
    public UpdateAuthorProfileAdminCommandValidator()
    {
        RuleFor(x => x.AuthorProfileId).GreaterThan(0);

        RuleFor(x => x.PenName)
            .NotEmpty()
            .MaximumLength(ApplicationConstants.MaxPenNameLength)
            .WithMessage(string.Format(
                ApplicationErrorConstants.PenNameLengthRequirementFormat,
                ApplicationConstants.MaxPenNameLength));

        RuleFor(x => x.Bio)
            .MaximumLength(ApplicationConstants.MaxBioLength)
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}
