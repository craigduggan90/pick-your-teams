using Teams.Common.Extensions;

namespace Teams.Common.Formatters;

/// <summary>Value formatting methods for <see langword="string"/> objects.</summary>
public static class StringFormatters
{
    /// <summary>Formats provided values in camelCase.</summary>
    /// <param name="input">The string to format.</param>
    /// <returns>The formatted string.</returns>
    /// <remarks>
    /// This can be used as a method group in conjunction with the Enum extension methods to format the output of
    /// GetName consistently:
    /// <code>
    /// // Without method group
    /// MyEnumType.GetName(name => name.ToCamelCase());
    /// // With method group
    /// MyEnumType.GetName(CamelCaseFormatter);
    /// </code>
    /// </remarks>
    public static string CamelCaseFormatter(string input)
        => input.ToCamelCase();
}