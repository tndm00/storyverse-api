namespace Library.Application.Commands.LibraryEntries.AddLibraryEntry;

/// <summary>Validates <see cref="AddLibraryEntryCommand"/> input before it reaches the handler.</summary>
public sealed class AddLibraryEntryCommandValidator : AbstractValidator<AddLibraryEntryCommand>
{
    public AddLibraryEntryCommandValidator()
    {
        // Story must be specified.
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        // Shelf status, when provided, must be one of the known values.
        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ShelfStatus))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
