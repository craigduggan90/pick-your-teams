namespace Teams.Domain.Exceptions;

public class PlayerAlreadyRegisteredException() : Exception(DefaultMessage)
{
    private const string DefaultMessage = "The player is already registered with the IdP.";
}