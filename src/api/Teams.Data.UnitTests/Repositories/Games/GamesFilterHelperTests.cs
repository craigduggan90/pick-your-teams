using Teams.Common.Providers.Identifiers;
using Teams.Data.Models;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories.Games;

public static class GamesFilterHelperTests
{
    private static IQueryable<Game> GetSeedData(int count) =>
        Enumerable.Range(1, count)
            .Select(i => SeedDataFactory.Games.Create(i, DummyUsers[i % 10]))
            .AsQueryable();

    private static readonly User[] DummyUsers = Enumerable.Range(1, 10)
        .Select(i =>
        {
            using var idFix = new IdentifierProviderContext($"u-{i:D8}");
            return new User($"display name {i}", $"ext|{i}", $"{i}@web.net", null);
        })
        .ToArray();

    public class ApplyLocationFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyLocationFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const string value = "Outer";
            var data = GetSeedData(30);
            var expected = data.Where(game => game.Location != null && game.Location.Contains(value));
            var filtered = data.ApplyLocationFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyStartTimeFromFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyStartTimeFromFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var data = GetSeedData(30);
            var value = data.Skip(14).First().StartTime;
            var expected = data.Where(game => game.StartTime >= value);
            var filtered = data.ApplyStartTimeFromFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyStartTimeToFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyStartTimeToFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var data = GetSeedData(30);
            var value = data.Skip(14).First().StartTime;
            var expected = data.Where(game => game.StartTime < value);
            var filtered = data.ApplyStartTimeToFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyDurationFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyDurationFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFromFilter_WhenOnlyFromProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(game => game.Duration >= 60);
            var filtered = data.ApplyDurationFilter(new RangeFilter<int>(60, null));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyToFilter_WhenOnlyToProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(game => game.Duration < 60);
            var filtered = data.ApplyDurationFilter(new RangeFilter<int>(null, 60));
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldApplyBothFilters_WhenFromAndToProvided()
        {
            var data = GetSeedData(30);
            var expected = data.Where(game => game.Duration >= 30 && game.Duration < 90);
            var filtered = data.ApplyDurationFilter(new RangeFilter<int>(30, 90));
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyTeamSizeFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyTeamSizeFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const int value = 5;
            var data = GetSeedData(30);
            var expected = data.Where(game => game.TeamSize == value);
            var filtered = data.ApplyTeamSizeFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyStatusFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyStatusFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            const GameStatusEnum value = GameStatusEnum.Finished;
            var data = GetSeedData(30);
            var expected = data.Where(game => game.Status == value);
            var filtered = data.ApplyStatusFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }
    }

    public class ApplyOrganiserIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyOrganiserIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldApplyFilter_WhenValueProvided()
        {
            var value = DummyUsers[3].Id;
            var data = GetSeedData(30);
            var expected = data.Where(game => game.OrganiserId == value);
            var filtered = data.ApplyOrganiserIdFilter(value);
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
            const string value = "u-participant-001";
            var data = GetSeedData(10).ToArray();

            // Add the target user as a player on two of the ten seeded games only.
            data[2].Players.Add(new Player(data[2].Id, value, 1000, PlayerTypeEnum.User, GameTeamEnum.None));
            data[7].Players.Add(new Player(data[7].Id, value, 1000, PlayerTypeEnum.User, GameTeamEnum.None));

            var queryable = data.AsQueryable();
            var expected = queryable.Where(game => game.Players.Any(player => player.UserId == value));
            var filtered = queryable.ApplyUserIdFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldNotMatchGame_WhenAnotherPlayerIsPresentButNotTheRequestedUser()
        {
            const string value = "u-participant-001";
            var data = GetSeedData(5).ToArray();
            data[0].Players.Add(new Player(data[0].Id, "some-other-user", 1000, PlayerTypeEnum.User, GameTeamEnum.None));

            var filtered = data.AsQueryable().ApplyUserIdFilter(value);

            Assert.Empty(filtered);
        }
    }

    public class ApplyOrganiserOrPlayerIdFilter
    {
        [Fact]
        public void ShouldNotApplyFilter_WhenNoValueProvided()
        {
            var data = GetSeedData(30);
            var filtered = data.ApplyOrganiserOrPlayerIdFilter(null);
            Assert.Same(data, filtered);
        }

        [Fact]
        public void ShouldMatchGame_WhenValueIsTheOrganiser()
        {
            var value = DummyUsers[3].Id;
            var data = GetSeedData(30);
            var expected = data.Where(game => game.OrganiserId == value);
            var filtered = data.ApplyOrganiserOrPlayerIdFilter(value);
            Assert.Equivalent(expected, filtered, true);
        }

        [Fact]
        public void ShouldMatchGame_WhenValueIsAPlayer()
        {
            const string value = "u-participant-001";
            var data = GetSeedData(10).ToArray();
            data[2].Players.Add(new Player(data[2].Id, value, 1000, PlayerTypeEnum.User, GameTeamEnum.None));

            var filtered = data.AsQueryable().ApplyOrganiserOrPlayerIdFilter(value);

            Assert.Single(filtered, game => game.Id == data[2].Id);
        }

        [Fact]
        public void ShouldMatchGameOnlyOnce_WhenValueIsBothTheOrganiserAndAPlayer()
        {
            var organiser = DummyUsers[3];
            var data = GetSeedData(10).ToArray();
            var game = data.First(g => g.OrganiserId == organiser.Id);
            game.Players.Add(new Player(game.Id, organiser.Id, 1000, PlayerTypeEnum.User, GameTeamEnum.None));

            var filtered = data.AsQueryable().ApplyOrganiserOrPlayerIdFilter(organiser.Id);

            Assert.Single(filtered, g => g.Id == game.Id);
        }

        [Fact]
        public void ShouldNotMatchGame_WhenValueIsNeitherOrganiserNorPlayer()
        {
            const string value = "u-nobody-here";
            var data = GetSeedData(5);

            var filtered = data.ApplyOrganiserOrPlayerIdFilter(value);

            Assert.Empty(filtered);
        }
    }
}