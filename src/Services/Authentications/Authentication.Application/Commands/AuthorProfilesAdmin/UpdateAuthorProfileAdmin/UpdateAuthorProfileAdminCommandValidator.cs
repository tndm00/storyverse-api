namespace Authentication.Application.Commands.AuthorProfilesAdmin.UpdateAuthorProfileAdmin;

public sealed class UpdateAuthorProfileAdminCommandValidator : AbstractValidator<UpdateAuthorProfileAdminCommand>
{
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
