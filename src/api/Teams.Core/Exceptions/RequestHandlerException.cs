using Teams.Core.Enums;

namespace Teams.Core.Exceptions;

public class RequestHandlerException : Exception
{
    public int StatusCode { get; }

    private RequestHandlerException(RequestType type, string message, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = type switch
        {
            RequestType.Command => 422,
            _ => 400
        };
    }

    public static RequestHandlerException ForCommandRequest(string message, Exception? innerException = null)
        => new(RequestType.Command, message, innerException);

    public static RequestHandlerException ForQueryRequest(string message, Exception? innerException = null)
        => new(RequestType.Query, message, innerException);
}