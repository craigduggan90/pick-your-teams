using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;

namespace Teams.Domain.Entities;

public class Game(
    string? location,
    DateTime startTime,
    DateTime? endTime,
    int teamSize)
    : EntityBase
{
    public string? Location { get; private set; } = location;

    public DateTime StartTime { get; private set; } = startTime;

    public DateTime? EndTime { get; private set; } = endTime;

    public GameStatusEnum Status { get; private set; } = GameStatusEnum.Scheduled;

    public int? HomeTeamRating { get; private set; }

    public int? AwayTeamRating { get; private set; }

    public GameTeamEnum? Winner { get; private set; }

    public int TeamSize { get; } = teamSize;

    public int MaxPlayers => TeamSize * 2;

    public void Update(string? location, DateTime? startTime, DateTime? endTime)
    {
        UpdateProperty(nameof(Location), location);
        UpdateProperty(nameof(StartTime), startTime);
        UpdateProperty(nameof(EndTime), endTime);
    }

    public void SetResult(GameTeamEnum winner)
    {
        if (Status == GameStatusEnum.Finished)
            return;
        UpdateProperty(nameof(Status), GameStatusEnum.Finished);
        UpdateProperty(nameof(Winner), winner);
    }

    public void Delete()
    {
        if (DateDeleted.HasValue)
            return;

        SetDateModified();
        SoftDelete();
    }

    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };
}