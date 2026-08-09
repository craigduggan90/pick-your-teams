namespace Teams.Domain.Exceptions;

public class TeamGenerationException(string? message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public static TeamGenerationException ForTooManyPlayersOnTeam(string team)
        => new($"Too many seeded players on the {team} team.");

    public static TeamGenerationException ForInvalidNumberOfSuggestionsRequested()
        => new("Invalid number of suggestions requested.");

    public static TeamGenerationException ForMinimumPlayerCountNotMet()
        => new("At least 2 players must be added to a game.");

    public static TeamGenerationException ForTooManyPlayersInGame()
        => new("Too many players added to the game.");
}