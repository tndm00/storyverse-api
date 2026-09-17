namespace Authentication.Application.Commands.AuthorProfilesAdmin.CreateAuthorProfileAdmin;

/// <summary>Validation rules for <see cref="CreateAuthorProfileAdminCommand"/>.</summary>
public sealed class CreateAuthorProfileAdminCommandValidator : AbstractValidator<CreateAuthorProfileAdminCommand>
{
    /// <summary>Requires a valid email, a strong password, and well-formed display name / pen name / bio.</summary>
    public CreateAuthorProfileAdminCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => email.IsValidEmail())
            .WithMessage(ApplicationErrorConstants.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must(password => password.IsStrongPassword())
            .WithMessage(string.Format(
                ApplicationErrorConstants.PasswordStrengthRequirementFormat,
                ApplicationConstants.MinPasswordLength));

        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);

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
