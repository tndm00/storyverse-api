namespace Library.Application.Commands.LibraryEntries.RemoveLibraryEntry;

public sealed class RemoveLibraryEntryCommandValidator : AbstractValidator<RemoveLibraryEntryCommand>
{
    public RemoveLibraryEntryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);
    }
}
