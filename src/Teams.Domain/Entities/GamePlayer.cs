using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.Entities;

public class GamePlayer(string gameId, string playerId, int rating, GameTeamEnum team)
{
    public GamePlayer(Game game, Player player, GameTeamEnum team = GameTeamEnum.None)
        : this(game.Id, player.Id, player.Rating, team)
    {
        Game = game;
        Player = player;
    }

    public string GameId { get; } = gameId;

    public Game Game
    {
        get => field ?? throw UninitializedPropertyException.For(nameof(Game));
        init;
    }

    public string PlayerId { get; } = playerId;

    public Player Player
    {
        get => field ?? throw UninitializedPropertyException.For(nameof(Player));
        init;
    }

    public int? Rating { get; private set; }

    public GameTeamEnum Team { get; private set; } = team;

    public void AssignTeam(GameTeamEnum team, int playerRating)
    {
        Team = team;
        Rating = playerRating;
    }
}