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

    public double? HomeTeamRatingChange => field ??= CalculateHomeTeamRatingChange();

    public int HomeTeamPlayerCount => Players.Count(player => player.Team == GameTeamEnum.Home);

    public int? AwayTeamRating { get; private set; }

    public double? AwayTeamRatingChange => field ??= CalculateAwayTeamRatingChange();

    public int AwayTeamPlayerCount => Players.Count(player => player.Team == GameTeamEnum.Away);

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

    public void SetResult(GameTeamEnum winner)
    {
        if (Status == GameStatusEnum.Finished)
            return;

        Status = GameStatusEnum.Finished;
        Winner = winner;

        // Sum the ratings since we'll use them a couple of times
        HomeTeamRating = Players.Where(player => player.Team == GameTeamEnum.Home).Sum(player => player.Rating);
        AwayTeamRating = Players.Where(player => player.Team == GameTeamEnum.Away).Sum(player => player.Rating);

        SetDateModified();
    }

    public override object AsSerializable()
        => new { Id, DateCreated, DateModified };

    private double? CalculateHomeTeamRatingChange()
    {
        if (HomeTeamRating is null || AwayTeamRating is null || Winner is null)
            return null;

        var outcome = Winner switch
        {
            GameTeamEnum.Home => GameResultEnum.Win,
            GameTeamEnum.Away => GameResultEnum.Loss,
            _ => GameResultEnum.Draw
        };

        return GetTeamRatingChange(HomeTeamRating.Value, AwayTeamRating.Value, outcome);
    }

    private double? CalculateAwayTeamRatingChange()
    {
        if (HomeTeamRating is null || AwayTeamRating is null || Winner is null)
            return null;

        var outcome = Winner switch
        {
            GameTeamEnum.Home => GameResultEnum.Loss,
            GameTeamEnum.Away => GameResultEnum.Win,
            _ => GameResultEnum.Draw
        };

        return GetTeamRatingChange(AwayTeamRating.Value, HomeTeamRating.Value, outcome);
    }

    private static double GetTeamRatingChange(int teamRatingSum, int opponentRatingSum, GameResultEnum outcome)
    {
        // Probability this team was expected to win, based on the ratings gap between the two teams (standard ELO
        // expected-score formula).
        var expectedWin = 1.0 / (1.0 + Math.Pow(10, (opponentRatingSum - teamRatingSum) / Constants.EloScalingFactor));

        // What actually happened, on the same 0-1 scale as expectedWin.
        var actualScore = outcome switch
        {
            GameResultEnum.Win => 1,
            GameResultEnum.Loss => 0,
            _ => 0.5
        };

        // How far off the prediction was, scaled by K — this is the total rating swing for the team, to be shared
        // across its players:
        return Constants.EloK * (actualScore - expectedWin);
    }
}