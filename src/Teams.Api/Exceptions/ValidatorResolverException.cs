namespace Teams.Core.Exceptions;

public class ValidatorResolverException(string message, Exception? innerException = null)
    : Exception(message, innerException);