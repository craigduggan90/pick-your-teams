using System.Text.Json;

namespace Teams.Common.Extensions;

/// <summary>Extension methods for the <see langword="string"/> type.</summary>
public static class StringExtensions
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
}