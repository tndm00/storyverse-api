namespace Content.Application.Text;

/// <summary>
/// Produces URL-safe, diacritic-free slugs from Vietnamese or English titles.
/// Used for story slugs, genre slugs, and tag de-duplication.
/// </summary>
public static class SlugGenerator
{
    private static readonly Regex NonSlugChars = new("[^a-z0-9]+", RegexOptions.Compiled);
    private static readonly Regex EdgeSeparators = new("^-+|-+$", RegexOptions.Compiled);

    /// <summary>Converts a title into a lowercase, hyphenated, ASCII-only slug.</summary>
    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Vietnamese "đ/Đ" has no combining form, so map it explicitly before
        // stripping the remaining diacritical marks.
        var normalized = value
            .Replace('đ', 'd')
            .Replace('Đ', 'd')
            .Normalize(NormalizationForm.FormD);

        // Strip diacritical marks left over after Unicode decomposition.
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        // Recompose, lowercase, then collapse any remaining non-alphanumeric runs
        // into single hyphens and trim leading/trailing hyphens.
        var ascii = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var slug = NonSlugChars.Replace(ascii, "-");
        return EdgeSeparators.Replace(slug, string.Empty);
    }
}
