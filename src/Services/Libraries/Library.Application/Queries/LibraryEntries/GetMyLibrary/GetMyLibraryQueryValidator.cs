namespace Library.Application.Queries.LibraryEntries.GetMyLibrary;

public sealed class GetMyLibraryQueryValidator : AbstractValidator<GetMyLibraryQuery>
{
    public GetMyLibraryQueryValidator()
    {
        RuleFor(x => x.ShelfStatus)
            .Must(value => ShelfStatusParser.TryParse(value, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.ShelfStatus))
            .WithMessage(ApplicationErrorConstants.InvalidShelfStatus);
    }
}
