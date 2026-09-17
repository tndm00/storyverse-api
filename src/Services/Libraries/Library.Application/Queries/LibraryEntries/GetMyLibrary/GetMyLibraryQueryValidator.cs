namespace Library.Application.Queries.LibraryEntries.GetMyLibrary;

/// <summary>Validates <see cref="GetMyLibraryQuery"/> input before it reaches the handler.</summary>
public sealed class GetMyLibraryQueryValidator : AbstractValidator<GetMyLibraryQuery>
{
    public GetMyLibraryQueryValidator()
    {
        // Shelf filter, when provided, must be a known value.
        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ShelfStatus))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
