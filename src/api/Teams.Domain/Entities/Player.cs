using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.Entities;

/// <summary>Represents a player in a game.</summary>
/// <param name="gameId">The unique identifier of the game in which the player is participating.</param>
/// <param name="userId">The unique identifier of the user with which this player is associated.</param>
/// <param name="displayName">The players display name.</param>
/// <param name="rating">The players rating.</param>
/// <param name="team">The team to which this player is assigned.</param>
public class Player(string gameId, string? userId, int rating, PlayerTypeEnum type, GameTeamEnum team)
    : EntityBase
{
    /// <summary>
    /// Create a new instance of the <see cref="Player"/> class representing a non-user player.
    /// </summary>
    /// <param name="game">The game in which this entity is a player.</param>
    /// <param name="displayName">The display name of the dummy user.</param>
    /// <param name="estimatedRating">The estimated rating for the dummy user.</param>
    public Player(Game game, string displayName, int estimatedRating)
        : this(game.Id, null, estimatedRating, PlayerTypeEnum.Dummy, GameTeamEnum.None)
    {
        Game = game;
        DisplayName = displayName;
    }

    /// <summary>
    /// Create a new instance of the <see cref="Player"/> class representing a user player.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="user">The user.</param>
    public Player(Game game, User user)
        : this(game.Id, user.Id, user.Rating, PlayerTypeEnum.User, GameTeamEnum.None)
    {
        Game = game;
        User = user;
    }

    /// <summary>
    /// The unique identifier of the game in which the player is participating.
    /// </summary>
    public string GameId { get; } = gameId;

    /// <summary>
    /// The display name of the game player, used for non-player participants and when the player has been deleted.
    /// </summary>
    public string? DisplayName { get; init; }

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

    /// <summary>Get the display name for the current player.</summary>
    public string? GetDisplayName() => User?.DisplayName ?? DisplayName;

    /// <summary>Set the player team and fix the player rating at time of assignment.</summary>
    /// <param name="team">The team to which the player is assigned.</param>
    /// <param name="rating">The player rating.</param>
    public void AssignTeam(GameTeamEnum team, int? rating)
    {
        UpdateProperty(nameof(Team), team);
        UpdateProperty(nameof(Rating), rating);
    }

    /// <summary>Clears the player team assignment.</summary>
    public void UnassignTeam()
    {
        UpdateProperty(nameof(Team), GameTeamEnum.None);
    }

    /// <summary>
    /// Sets the rating change as a result of the game.
    /// </summary>
    /// <param name="teamRating">The total rating of the players team.</param>
    /// <param name="teamChange">The total rating change to apply to the players team.</param>
    /// <param name="teamSize">The number of players on the team.</param>
    /// <remarks>
    /// This does not alter the <see cref="Rating"/> property of this player, but rather serves as a record of the
    /// change that has been applied to the associated user (if any).
    /// </remarks>
    public void SetRatingChange(int teamRating, double teamChange, int teamSize)
    {
        var weight = teamChange > 0
                ? (double)(teamRating - Rating) / (teamRating * (teamSize - 1))
                : (double)Rating / teamRating;
        RatingChange = (int)Math.Round(teamChange * weight);
        SetDateModified();
    }

    /// <inheritdoc />
    public override object AsSerializable() =>
        new { Id, GameId, UserId, Team, DateCreated, DateModified };
}