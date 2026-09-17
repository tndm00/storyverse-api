namespace Library.Application.Commands.LibraryEntries.RemoveLibraryEntry;

/// <summary>Validates <see cref="RemoveLibraryEntryCommand"/> input before it reaches the handler.</summary>
public sealed class RemoveLibraryEntryCommandValidator : AbstractValidator<RemoveLibraryEntryCommand>
{
    public RemoveLibraryEntryCommandValidator()
    {
        // Story must be specified.
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);
    }
}
