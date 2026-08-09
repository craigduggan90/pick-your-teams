namespace Teams.Domain.Exceptions;

/// <summary>Represents an error resulting from an uninitialized navigation property.</summary>
public class UninitializedPropertyException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="UninitializedPropertyException"/> class.</summary>
    public UninitializedPropertyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UninitializedPropertyException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public UninitializedPropertyException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UninitializedPropertyException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UninitializedPropertyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>Create a new instance of <see cref="UninitializedPropertyException"/>.</summary>
    /// <param name="propertyName">The name of the uninitialized property.</param>
    public static UninitializedPropertyException For(string propertyName)
        => new($"Uninitialized property: {propertyName}");
}