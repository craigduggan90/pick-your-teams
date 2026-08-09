using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;

namespace Teams.Domain.Entities;

public class Invitation(string gameId, string? userId, string emailAddress) : EntityBase
{
    public string GameId { get; init; } = gameId;

    public string? UserId { get; init; } = userId;

    public string EmailAddress { get; init; } = emailAddress;

    public InvitationStatusEnum Status { get; private set; } = InvitationStatusEnum.None;

    public string? ErrorMessage { get; private set; }

    public User? User { get; init; }

    public Game Game
    {
        get => field ?? throw new UninitializedPropertyException(nameof(Game));
        init;
    }

    public void Accept()
    {
        if (Status != InvitationStatusEnum.None)
            return;

        UpdateProperty(nameof(Status), InvitationStatusEnum.Accepted);
    }

    public void Decline()
    {
        if (Status != InvitationStatusEnum.None)
            return;

        UpdateProperty(nameof(Status), InvitationStatusEnum.Declined);
    }

    public void DispatchError(string errorMessage)
    {
        if (Status != InvitationStatusEnum.None)
            return;

        UpdateProperty(nameof(Status), InvitationStatusEnum.Failed);
        UpdateProperty(nameof(ErrorMessage), errorMessage);
    }

    public override object AsSerializable() => new { Id, GameId, Invitee = UserId ?? EmailAddress, DateCreated };
}