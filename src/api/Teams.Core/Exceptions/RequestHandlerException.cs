namespace Teams.Core.Exceptions;

/// <summary>Represents an error raised when trying to resolve the handler for a request.</summary>
public class RequestHandlerException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="RequestHandlerException"/> class.</summary>
    internal RequestHandlerException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHandlerException"/> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    internal RequestHandlerException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHandlerException"/> class with a specified message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or a null reference if no inner exception is
    /// specified.
    /// </param>
    internal RequestHandlerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Get an instance of <see cref="RequestHandlerException"/> representing a failure to resolve the handler for the
    /// specified request type.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be resolved.</param>
    public static RequestHandlerException ForRequest(Type requestType)
        => new($"Unable to resolve handler for '{requestType.Name}' request.");

    /// <summary>
    /// Get an instance of <see cref="RequestHandlerException"/> representing a failure to instantiate the handler for
    /// the specified request type.
    /// </summary>
    /// <param name="requestType">The type of request for which the handler could not be instantiated.</param>
    public static RequestHandlerException ForFailedInstantiation(Type requestType)
        => new($"Unable to instantiate handler for '{requestType.Name}' request.");
}