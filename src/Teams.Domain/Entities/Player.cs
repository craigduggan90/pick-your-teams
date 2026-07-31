using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.Entities;

/// <summary>Represents a player in a game.</summary>
/// <param name="gameId"></param>
/// <param name="userId"></param>
/// <param name="displayName"></param>
/// <param name="rating"></param>
/// <param name="team"></param>
public class Player(string gameId, string? userId, string displayName, int rating, PlayerTypeEnum type, GameTeamEnum team)
    : EntityBase
{
    /// <summary>
    /// Create a new instance of the <see cref="Player"/> class representing a non-user player.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="displayName">The display name of the dummy user.</param>
    /// <param name="estimatedRating">The estimated rating for the dummy user.</param>
    public Player(string gameId, string displayName, int estimatedRating)
        : this(gameId, null, displayName, Constants.StartingElo, PlayerTypeEnum.Dummy, GameTeamEnum.None)
    {
        Rating = estimatedRating;
    }

    /// <summary>
    /// Create a new instance of the <see cref="Player"/> class representing a user player.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="user">The user.</param>
    public Player(Game game, User user)
        : this(game.Id, user.Id, user.Tag, user.Rating, PlayerTypeEnum.User, GameTeamEnum.None)
    {
    }

    /// <summary>
    /// The unique identifier of the game in which the player is participating.
    /// </summary>
    public string GameId { get; } = gameId;

    /// <summary>
    /// The display name of the game player, used for non-player participants and when the player has been deleted.
    /// </summary>
    public string DisplayName { get; init; } = displayName;

    /// <summary>
    /// The unique identifier of the user. <c>null</c> when the player is a dummy player.
    /// </summary>
    public string? UserId { get; } = userId;

    /// <summary>
    /// The user participating. <c>null</c> when the player is a dummy player.
    /// </summary>
    public User? User { get; init; }

    /// <summary>The player rating, immutable once <see cref="RatingChange"/> has been calculated.</summary>
    public int Rating { get; private set; } = rating;

    /// <summary>The player rating adjustment applied as a result of this game.</summary>
    public int? RatingChange { get; private set; }

    public PlayerTypeEnum Type { get; } = type;

    /// <summary>
    /// The team to which the player is assigned (Home, Away, None).
    /// </summary>
    public GameTeamEnum Team { get; private set; } = team;

    /// <summary>The game.</summary>
    /// <exception cref="UninitializedPropertyException">when the game is not available.</exception>
    public Game Game
    {
        get => field ?? throw UninitializedPropertyException.For(nameof(Game));
        init;
    }

    public void AssignTeam(GameTeamEnum team, int playerRating)
    {
        Team = team;
        Rating = playerRating;
    }

    /// <inheritdoc />
    public override object AsSerializable() =>
        new { Id, GameId, UserId, Team, DateCreated, DateModified };
}