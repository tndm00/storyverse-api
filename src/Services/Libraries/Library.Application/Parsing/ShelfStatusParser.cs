namespace Library.Application.Parsing;

/// <summary>
/// Single place that turns the wire string for a shelf into the
/// <see cref="ShelfStatus"/> enum, so every command/query parses it the same way.
/// </summary>
public static class ShelfStatusParser
{
    /// <summary>Parses a required shelf value; throws <see cref="BadRequestException"/> when unrecognized.</summary>
    public static ShelfStatus Parse(string value)
    {
        if (!TryParse(value, out var status))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidShelfStatus);
        }

        return status;
    }

    /// <summary>Parses an optional shelf value, falling back to <paramref name="fallback"/> when blank.</summary>
    public static ShelfStatus ParseOrDefault(string value, ShelfStatus fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return Parse(value);
    }

    /// <summary>Parses an optional filter value; blank means "no filter" (null), unrecognized throws.</summary>
    public static ShelfStatus? ParseFilter(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Parse(value);
    }

    /// <summary>Attempts to parse a shelf value, matching case-insensitively against defined enum names.</summary>
    public static bool TryParse(string value, out ShelfStatus status)
    {
        return Enum.TryParse(value, ignoreCase: true, out status) && Enum.IsDefined(status);
    }
}
