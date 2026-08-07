namespace Be.StoryVerse.Core.Extensions.Validations;

/// <summary>
/// Small, reusable validation helpers referenced by FluentValidation rules
/// across services, per code-standard.md section 6.
/// </summary>
public static class StringValidationExtensions
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Basic email shape check. Full deliverability is not validated here.
    /// </summary>
    public static bool IsValidEmail(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && EmailPattern.IsMatch(value);
    }

    /// <summary>
    /// Enforces a minimum password strength: at least 8 characters, one letter
    /// and one digit. Kept intentionally simple for Phase 1.
    /// </summary>
    public static bool IsStrongPassword(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 8)
        {
            return false;
        }

        var hasLetter = false;
        var hasDigit = false;

        foreach (var c in value)
        {
            if (char.IsLetter(c))
            {
                hasLetter = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
        }

        return hasLetter && hasDigit;
    }
}
