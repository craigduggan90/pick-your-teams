namespace Teams.Common.Exceptions;

/// <summary>Represents an error occurring when converting to/from an enum type.</summary>
public class EnumConversionException : Exception
{
    internal const string DefaultMessage = "An error occurred when converting an enum value.";

    /// <summary>Initializes a new instance of the <see cref="EnumConversionException"/> class.</summary>
    public EnumConversionException()
        : base(DefaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumConversionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    internal EnumConversionException(string? message)
        : base(message)
    {
    }

    /// <summary>Get an <see cref="EnumConversionException"/> representing an undefined value.</summary>
    /// <param name="enumType">The type of the enum.</param>
    /// <param name="value">The value for which there was no corresponding enum entry.</param>
    /// <typeparam name="TValue">The type of `value`.</typeparam>
    internal static EnumConversionException ForUndefinedValue<TValue>(Type enumType, TValue value)
        where TValue : struct =>
        new($"No {enumType.Name} found with value {value}.");
}