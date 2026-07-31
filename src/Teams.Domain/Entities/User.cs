using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.Entities;

/// <summary>Represents a player/user.</summary>
public class User(string tag, string displayName, string emailAddress, string? mobile) : EntityBase
{
    /// <summary>The identifier of the user in the external IDP service.</summary>
    /// <remarks>Dummy or placeholder players will not have an ExternalId.</remarks>
    public string? ExternalId { get; private set; }

    /// <summary> The players email address, optionally used for notifications and reminders.</summary>
    public string EmailAddress { get; private set; } = emailAddress;

    /// <summary> The players mobile phone number, optionally used for notifications and reminders.</summary>
    public string? Mobile { get; private set; } = mobile;

    /// <summary>The users tag or handle.</summary>
    public string Tag { get; private set; } = tag;

    /// <summary>The users display name.</summary>
    public string DisplayName { get; private set; } = displayName;

    /// <summary>The users current game rating.</summary>
    /// <remarks>Default 1000.</remarks>
    public int Rating { get; private set; } = 1000;

    /// <summary>The games with which this player is associated.</summary>
    public ICollection<Player> Participation { get; private set; } = new List<Player>();

    /// <summary>Update the properties of this player.</summary>
    /// <param name="tag">The users tag, or display name.</param>
    /// <param name="emailAddress">The players email address.</param>
    /// <param name="mobile">The players mobile phone number.</param>
    public void Update(string? tag, string? emailAddress, string? mobile)
    {
        UpdateProperty(nameof(Tag), tag);
        UpdateProperty(nameof(EmailAddress), emailAddress);
        UpdateProperty(nameof(Mobile), mobile);
    }

    /// <summary>Sets the players IdP information. May be called only once per player.</summary>
    /// <param name="externalId">The unique identifier of the user in the IdP.</param>
    /// <exception cref="PlayerAlreadyRegisteredException">when the player is already associated with an IdP user.</exception>
    public void SetExternalId(string externalId)
    {
        if (ExternalId is not null)
            throw new PlayerAlreadyRegisteredException();

        UpdateProperty(nameof(ExternalId), externalId);
    }

    /// <summary>Adjusts this player's rating following a completed game, using team-sum ELO.</summary>
    /// <param name="playerTeamRatingSum">Sum of ratings for every player on this player's team, including this player.</param>
    /// <param name="opponentTeamRatingSum">Sum of ratings for every player on the opposing team.</param>
    /// <param name="teamSize">Number of players on this player's team.</param>
    /// <param name="outcome">The result of the game from this player's team's perspective.</param>
    public void AdjustRating(
        int playerTeamRatingSum,
        int opponentTeamRatingSum,
        int teamSize,
        GameResultEnum outcome)
    {
        // Probability this team was expected to win, based on the ratings gap between the two teams (standard ELO
        // expected-score formula).
        var expectedWin = 1.0 / (1.0 + Math.Pow(10, (opponentTeamRatingSum - playerTeamRatingSum) / Constants.EloScalingFactor));

        // What actually happened, on the same 0-1 scale as expectedWin.
        var actualScore = outcome switch
        {
            GameResultEnum.Win => 1,
            GameResultEnum.Loss => 0,
            _ => 0.5
        };

        // How far off the prediction was, scaled by K — this is the total rating swing for the team, to be shared
        // across its players.
        var teamDelta = Constants.EloK * (actualScore - expectedWin);

        // This player's share of teamDelta: lower-rated players on the team get a bigger slice, higher-rated players
        // get a smaller one. Weights across the whole team sum to 1.
        var weight = (double)(playerTeamRatingSum - Rating) / (playerTeamRatingSum * (teamSize - 1));

        Rating += (int)Math.Round(teamDelta * weight);
    }

    /// <inheritdoc />
    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };
}