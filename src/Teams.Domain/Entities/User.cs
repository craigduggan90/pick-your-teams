using Teams.Domain.Entities.Abstract;

namespace Teams.Domain.Entities;

/// <summary>Represents a player/user.</summary>
public class User(string tag, string displayName, string externalId, string emailAddress, string? mobile) : EntityBase
{
    /// <summary>The identifier of the user in the external IDP service.</summary>
    public string? ExternalId { get; } = externalId;

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

    public void ApplyRatingChange(int ratingChange)
    {
        UpdateProperty(nameof(Rating), Rating + ratingChange);
    }

    /// <inheritdoc />
    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };
}