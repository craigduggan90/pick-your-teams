using System.Text.Json;
using System.Text.RegularExpressions;

namespace Teams.Common.Extensions;

/// <summary>Extension methods for the <see langword="string"/> type.</summary>
public static partial class StringExtensions
{
    /// <summary>Formats a string in camelCase.</summary>
    /// <param name="input">The string to format.</param>
    /// <returns>The formatted string.</returns>
    public static string ToCamelCase(this string input)
        => JsonNamingPolicy.CamelCase.ConvertName(input);

    /// <summary>
    /// Formats a string in kebab-case (lowercase).
    /// </summary>
    /// <param name="input">The string to format.</param>
    /// <returns>The formatted string.</returns>
    public static string ToKebabCaseLower(this string input)
        => JsonNamingPolicy.KebabCaseLower.ConvertName(input);

    public static bool IsValidEmail(this string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith('.'))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(trimmedEmail);
            return addr.Address == trimmedEmail;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }

    public static bool IsValidTag(this string tag) => TagRegex().IsMatch(tag);

    [GeneratedRegex(Constants.TagRegexPattern)]
    private static partial Regex TagRegex();
}