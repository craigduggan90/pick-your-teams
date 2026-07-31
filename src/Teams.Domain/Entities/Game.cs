using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;

namespace Teams.Domain.Entities;

public class Game(
    string? location,
    DateTime startTime,
    int duration,
    int teamSize)
    : EntityBase
{
    public string? Location { get; private set; } = location;

    public DateTime StartTime { get; private set; } = startTime;

    public int Duration { get; private set; } = duration;

    public GameStatusEnum Status { get; private set; } = GameStatusEnum.Scheduled;

    public int? HomeTeamRating { get; private set; }

    public int? AwayTeamRating { get; private set; }

    public GameTeamEnum? Winner { get; private set; }

    public int TeamSize { get; } = teamSize;

    public int MaxPlayers => TeamSize * 2;

    public ICollection<Player> Players { get; private set; } = new List<Player>();

    public void Update(string? location, DateTime? startTime, int? duration)
    {
        UpdateProperty(nameof(Location), location);
        UpdateProperty(nameof(StartTime), startTime);
        UpdateProperty(nameof(Duration), duration);
    }

    public Player AddDummyPlayer(string displayName, int estimatedRating)
    {
        var player = new Player(Id, displayName, estimatedRating);
        Players.Add(player);
        return player;
    }

    public void SetResult(GameTeamEnum winner)
    {
        if (Status == GameStatusEnum.Finished)
            return;

        UpdateProperty(nameof(Status), GameStatusEnum.Finished);
        UpdateProperty(nameof(Winner), winner);
    }

    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };
}