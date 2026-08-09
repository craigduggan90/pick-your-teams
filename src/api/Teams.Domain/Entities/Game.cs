using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;
using Teams.Domain.Exceptions;
using Teams.Domain.Models;

namespace Teams.Domain.Entities;

public class Game(
    string organiserId,
    string? location,
    DateTime startTime,
    int duration,
    int teamSize)
    : EntityBase
{
    public string? Location { get; private set; } = location;

    public DateTime StartTime { get; private set; } = startTime;

    public int Duration { get; private set; } = duration;

    public string OrganiserId { get; private set; } = organiserId;

    public User Organiser
    {
        get => field ?? throw UninitializedPropertyException.For(nameof(Organiser));
        init;
    }

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
        UpdateHomeTeamRating();
        UpdateAwayTeamRating();

        SetDateModified();
    }

    public void UpdateHomeTeamRating()
    {
        var rating = GetSumOfPlayerRatings(Players.Where(player => player.Team == GameTeamEnum.Home));
        UpdateProperty(nameof(HomeTeamRating), rating);
    }

    public void UpdateAwayTeamRating()
    {
        var rating = GetSumOfPlayerRatings(Players.Where(player => player.Team == GameTeamEnum.Away));
        UpdateProperty(nameof(AwayTeamRating), rating);
    }

    public IReadOnlyCollection<TeamSuggestion> GetTeamSuggestions(
        IReadOnlyCollection<string> homeTeamSeedIds,
        IReadOnlyCollection<string> awayTeamSeedIds,
        int differentialThreshold,
        int maxSuggestions)
    {
        if (maxSuggestions is <= 0 or > Constants.MaximumGeneratedTeamSuggestionCount)
            throw TeamGenerationException.ForInvalidNumberOfSuggestionsRequested();

        if (Players.Count <= 1)
            throw TeamGenerationException.ForMinimumPlayerCountNotMet();

        if (Players.Count > MaxPlayers)
            throw TeamGenerationException.ForTooManyPlayersInGame();

        // Copy Players so we don't accidentally operate on the entity copy
        var unassigned = new Player[Players.Count];
        Players.CopyTo(unassigned, 0);

        // Take the seeded players and remove them from the unassigned array
        var homeTeam = unassigned.Where(player => homeTeamSeedIds.Contains(player.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        var awayTeam = unassigned.Where(player => awayTeamSeedIds.Contains(player.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        unassigned = [.. unassigned.Except(homeTeam.Union(awayTeam))];

        if (homeTeam.Count > TeamSize)
            throw TeamGenerationException.ForTooManyPlayersOnTeam("home");

        if (awayTeam.Count > TeamSize)
            throw TeamGenerationException.ForTooManyPlayersOnTeam("away");

        var totalRating = GetSumOfPlayerRatings(Players);
        var homeSeedRating = GetSumOfPlayerRatings(homeTeam);
        var awaySeedRating = GetSumOfPlayerRatings(awayTeam);

        // If the seeded teams leave no unassigned players, just return those teams - if the seeded teams don't meet the
        // differential limit, return an empty collection.
        if (unassigned.Length == 0)
        {
            var fullySeededSuggestion = TryBuildFromFullySeededTeams(
                homeTeam, awayTeam, homeSeedRating, awaySeedRating, differentialThreshold);
            return fullySeededSuggestion is null
                ? []
                : [fullySeededSuggestion.Value];
        }

        // Odd total counts give Home the extra player.
        var homeSize = (Players.Count + 1) / 2;
        var homeNeeded = homeSize - homeTeam.Count;

        // The seeded teams are already too imbalanced in size for any split of the remaining players to work
        if (homeNeeded < 0 || homeNeeded > unassigned.Length)
            throw TeamGenerationException.ForTooManyPlayersOnTeam(homeNeeded < 0 ? "home" : "away");

        return GenerateTeamSuggestions(
            unassigned, homeTeam, awayTeam, homeNeeded, homeSeedRating, totalRating, differentialThreshold, maxSuggestions);
    }

    /// <summary>
    /// Walks every possible Home/Away split of <paramref name="unassigned"/> and reservoir-samples up to
    /// <paramref name="maxSuggestions"/> of the ones that meet <paramref name="differentialThreshold"/>.
    /// </summary>
    private static TeamSuggestion[] GenerateTeamSuggestions(
        Player[] unassigned,
        List<Player> homeTeam,
        List<Player> awayTeam,
        int homeNeeded,
        int homeSeedRating,
        int totalRating,
        int differentialThreshold,
        int maxSuggestions)
    {
        var reservoir = new TeamSuggestion[maxSuggestions];
        var reservoirCount = 0; // how many slots in the reservoir are actually filled so far
        var seenCount = 0;      // how many *matching* (within-threshold) suggestions we've encountered total

        foreach (var mask in GetCombinations(unassigned.Length, homeNeeded))
        {
            var (homeExtra, awayExtra, homeRating) = SplitByMask(mask, unassigned, homeNeeded, homeSeedRating);

            var awayRating = totalRating - homeRating;
            var differential = Math.Abs(homeRating - awayRating);
            if (differential > differentialThreshold)
                continue;

            var suggestion = new TeamSuggestion(
                [.. homeTeam, .. homeExtra],
                [.. awayTeam, .. awayExtra],
                homeRating,
                awayRating,
                differential);

            TryAddToReservoir(reservoir, ref reservoirCount, ref seenCount, maxSuggestions, suggestion);
        }

        return [.. reservoir.Take(reservoirCount)];
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

    /// <summary>
    /// Generates every k-combination of the numbers 0..n-1, each expressed as a bitmask where bit i
    /// being set means "index i is included in this combination" - via Gosper's hack.
    /// </summary>
    /// <param name="n">The size of the set being chosen from (e.g. the number of unassigned players).</param>
    /// <param name="k">How many of those n items each combination should include (e.g. how many go to Home).</param>
    /// <returns>
    /// Every int with exactly k bits set within the low n bits, in ascending numeric order - one per
    /// possible combination. There are C(n, k) of them in total.
    /// </returns>
    private static IEnumerable<int> GetCombinations(int n, int k)
    {
        if (k == 0)
        {
            yield return 0;
            yield break;
        }

        var mask = (1 << k) - 1;
        var limit = 1 << n;

        while (mask < limit)
        {
            yield return mask;

            var c = mask & -mask;
            var r = mask + c;
            mask = (((r ^ mask) >> 2) / c) | r;
        }
    }

    /// <summary>Splits <paramref name="unassigned"/> into Home/Away according to <paramref name="mask"/>,
    /// and sums the resulting Home rating (starting from the already-seeded Home total).</summary>
    private static (Player[] Home, Player[] Away, int HomeRating) SplitByMask(
        int mask, Player[] unassigned, int homeCount, int homeSeedRating)
    {
        var home = new List<Player>(homeCount);
        var away = new List<Player>(unassigned.Length - homeCount);
        var homeRating = homeSeedRating;

        for (var i = 0; i < unassigned.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                home.Add(unassigned[i]);
                homeRating += unassigned[i].Rating;
                continue;
            }

            away.Add(unassigned[i]);
        }

        return ([.. home], [.. away], homeRating);
    }

    /// <summary>
    /// Reservoir sampling (Algorithm R): gives every matching suggestion an equal chance of ending up in the
    /// final result, without needing to know the total match count upfront.
    /// </summary>
    private static void TryAddToReservoir(
        TeamSuggestion[] reservoir, ref int reservoirCount, ref int seenCount, int maxSuggestions, TeamSuggestion suggestion)
    {
        seenCount++;

        if (reservoirCount < maxSuggestions)
        {
            reservoir[reservoirCount++] = suggestion;
            return;
        }

        var replaceAt = Random.Shared.Next(seenCount);
        if (replaceAt < maxSuggestions)
            reservoir[replaceAt] = suggestion;
    }

    /// <summary>
    /// Builds the single possible suggestion when seeding has already assigned every player to a team,
    /// or null if that pre-seeded split doesn't meet <paramref name="differentialThreshold"/>.
    /// </summary>
    private static TeamSuggestion? TryBuildFromFullySeededTeams(
        List<Player> homeTeam, List<Player> awayTeam, int homeSeedRating, int awaySeedRating, int differentialThreshold)
    {
        var differential = Math.Abs(homeSeedRating - awaySeedRating);
        return differential <= differentialThreshold
            ? new TeamSuggestion(homeTeam, awayTeam, homeSeedRating, awaySeedRating, differential)
            : null;
    }

    private static int GetSumOfPlayerRatings(IEnumerable<Player> players) => players.Sum(player => player.Rating);

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
}