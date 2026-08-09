using Teams.Common.Exceptions;

namespace Teams.Common.Extensions;

/// <summary>Extension methods for <see langword="int"/> values.</summary>
public static class IntExtensions
{
    /// <summary>Get the enum entry representing a given integer.</summary>
    /// <param name="value">The integer to convert.</param>
    /// <typeparam name="TEnum">The type of enum to which the value is converted.</typeparam>
    /// <returns>The enum entry representing the value.</returns>
    /// <exception cref="EnumConversionException">when the value was not found in the enum.</exception>
    public static TEnum ToEnum<TEnum>(this int value)
        where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), value)
            ? (TEnum)Enum.ToObject(typeof(TEnum), value)
            : throw EnumConversionException.ForUndefinedValue(typeof(TEnum), value);

    /// <summary>Get the enum entry representing a given integer.</summary>
    /// <param name="value">The integer to convert.</param>
    /// <typeparam name="TEnum">The type of enum to which the value is converted.</typeparam>
    /// <returns>The enum entry representing the value, or null if the input value is null.</returns>
    public static TEnum? ToEnum<TEnum>(this int? value)
        where TEnum : struct, Enum =>
        value?.ToEnum<TEnum>();
}