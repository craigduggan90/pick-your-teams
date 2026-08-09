namespace Teams.Common.Extensions;

/// <summary>Extension methods for base <see cref="Enum"/> types.</summary>
public static class EnumExtensions
{
    /// <summary>Gets the name value of an enum entry.</summary>
    /// <param name="value">The enum entry.</param>
    /// <typeparam name="TEnum">The type of enum of which `value` is an entry.</typeparam>
    /// <returns>The name if available; otherwise null.</returns>
    public static string? GetName<TEnum>(this TEnum value)
        where TEnum : struct, Enum
        => Enum.GetName(value);
}