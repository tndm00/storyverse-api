namespace Library.Application.Commands.LibraryEntries.ChangeShelfStatus;

/// <summary>Validates <see cref="ChangeShelfStatusCommand"/> input before it reaches the handler.</summary>
public sealed class ChangeShelfStatusCommandValidator : AbstractValidator<ChangeShelfStatusCommand>
{
    public ChangeShelfStatusCommandValidator()
    {
        // Story must be specified.
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        // Target shelf must be a known value (required for this command, unlike Add).
        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
