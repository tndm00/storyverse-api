namespace Authentication.Application.Commands.CreateAuthorProfile;

/// <summary>Validation rules for <see cref="CreateAuthorProfileCommand"/>.</summary>
public sealed class CreateAuthorProfileCommandValidator : AbstractValidator<CreateAuthorProfileCommand>
{
    /// <summary>Requires a well-formed pen name and bio.</summary>
    public CreateAuthorProfileCommandValidator()
    {
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
