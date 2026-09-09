namespace Library.Application.Commands.LibraryEntries.AddLibraryEntry;

public sealed class AddLibraryEntryCommandValidator : AbstractValidator<AddLibraryEntryCommand>
{
    public AddLibraryEntryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ShelfStatus))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
