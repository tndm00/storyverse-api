namespace Library.Application.Commands.LibraryEntries.ChangeShelfStatus;

public sealed class ChangeShelfStatusCommandValidator : AbstractValidator<ChangeShelfStatusCommand>
{
    public ChangeShelfStatusCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
