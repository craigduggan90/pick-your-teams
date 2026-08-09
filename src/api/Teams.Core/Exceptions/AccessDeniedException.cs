namespace Teams.Core.Exceptions;

public class AccessDeniedException : Exception
{
    private AccessDeniedException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public static AccessDeniedException ForOrganiserOnly()
        => new("Action only available to game organiser.");

    public static AccessDeniedException ForSelfOnly()
        => new("Action only available to subject user.");

    public static AccessDeniedException ForOrganiserOrSelfOnly()
        => new("Action only available to game organiser or subject user.");
}