namespace Teams.Domain.Exceptions;

/// <summary>Represents an error arising when attempting to update the properties of an entity.</summary>
public class EntityUpdateException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="EntityUpdateException"/> class.</summary>
    public EntityUpdateException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EntityUpdateException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EntityUpdateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>Represents an error arising from attempting to write to a non-existent or inaccessible property.</summary>
    /// <param name="parentType">The type containing the property to which the user attempted to write.</param>
    /// <param name="propertyName">The name of the property to which the user attempted to write.</param>
    public static EntityUpdateException ForMissingProperty(Type parentType, string propertyName)
        => new($"Property not found or inaccessible: '{parentType.Name}.{propertyName}'.");

    /// <summary>Represents an error arising from attempting to assign an invalid value to a property.</summary>
    /// <param name="parentType">The type containing the property to which the user attempted to write.</param>
    /// <param name="propertyName">The name of the property to which the user attempted to write.</param>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="valueType">The type of the value that the user attempted to assign to the property.</param>
    public static EntityUpdateException ForIncorrectType(Type parentType, string propertyName, Type propertyType, Type valueType)
        => new($"Cannot assign {valueType.Name} to {propertyType.Name} member: '{parentType.Name}.{propertyName}'.");
}