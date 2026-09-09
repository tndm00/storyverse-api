namespace Authentication.Application.Commands.CreateAuthorProfile;

public sealed class CreateAuthorProfileCommandValidator : AbstractValidator<CreateAuthorProfileCommand>
{
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
