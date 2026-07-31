namespace Teams.Core.Exceptions;

public class IdpException(string? message = null, Exception? innerException = null)
    : Exception(message, innerException)
{
}