using Teams.Common.Providers.Identifiers;
using Teams.Common.Providers.Temporal;
using Teams.Data.Models;
using Teams.Data.Repositories.Players;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Players;

public static class PlayersFilterHelperTests
{
    private static readonly DateTime BaseDate = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly string[] GameIds = ["g-00000001", "g-00000002", "g-00000003"];
    private static readonly PlayerTypeEnum[] Types = [PlayerTypeEnum.Dummy, PlayerTypeEnum.User];
    private static readonly GameTeamEnum[] TeamValues = [GameTeamEnum.None, GameTeamEnum.Home, GameTeamEnum.Away];

    private static readonly Game PlaceholderGame = new("organiser-id", null, DateTime.UtcNow, 60, 5);

    private static IQueryable<Player> GetSeedData(int count) => Enumerable.Range(1, count)
        .Select(i =>
        {
            using var idFix = new IdentifierProviderContext($"p-{i:D8}");
            using var dtFix = new DateTimeOffsetProviderContext(BaseDate.AddDays(i));

            var type = Types[i % Types.Length];
            var userId = type == PlayerTypeEnum.User ? $"u-{i:D8}" : null;
            var rating = 1000 + i * (i % 2 == 0 ? 10 : -10);

            return new Player(GameIds[i % GameIds.Length], userId, $"display name {i:D8}", rating, type, TeamValues[i % TeamValues.Length])
            {
                Game = PlaceholderGame
            };
        })
        .AsQueryable();

    public class ApplyGameIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyGameIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var value = GameIds[1];
            var data = GetSeedData(30);
            var expected = data.Where(player => player.GameId == value);
            var filtered = data.ApplyGameIdFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyDisplayNameFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyDisplayNameFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const string value = "00000015";
            var data = GetSeedData(30);
            var expected = data.Where(player => player.GetDisplayName.Contains(value));
            var filtered = data.ApplyDisplayNameFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyUserIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyUserIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            // Index 3 is odd, so Type is User (Types[3 % 2] = User) and carries a UserId.
            const string value = "u-00000003";
            var data = GetSeedData(30);
            var expected = data.Where(player => player.UserId == value);
            var filtered = data.ApplyUserIdFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyTeamFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyTeamFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const GameTeamEnum value = GameTeamEnum.Home;
            var data = GetSeedData(30);
            var expected = data.Where(player => player.Team == value);
            var filtered = data.ApplyTeamFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyTypeFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyTypeFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const PlayerTypeEnum value = PlayerTypeEnum.Dummy;
            var data = GetSeedData(30);
            var expected = data.Where(player => player.Type == value);
            var filtered = data.ApplyTypeFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyRatingFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyRatingFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFromFilter_WhenOnlyFromProvided()
        {
            var data = GetSeedData(30);
            var from = data.OrderBy(player => player.Rating).Skip(9).First().Rating;
            var expected = data.Where(player => player.Rating >= from);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(from, null));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyToFilter_WhenOnlyToProvided()
        {
            var data = GetSeedData(30);
            var to = data.OrderBy(player => player.Rating).Skip(9).First().Rating;
            var expected = data.Where(player => player.Rating < to);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(null, to));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyBothFilters_WhenFromAndToProvided()
        {
            var data = GetSeedData(30);
            var ordered = data.OrderBy(player => player.Rating).ToList();
            var from = ordered[9].Rating;
            var to = ordered[19].Rating;
            var expected = data.Where(player => player.Rating >= from && player.Rating < to);
            var filtered = data.ApplyRatingFilter(new RangeFilter<int>(from, to));
            Assert.Equivalent(expected, filtered, true);
        }
    }
}