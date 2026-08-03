using Teams.Data.UnitTests.TestHelpers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Data.UnitTests.Repositories;

public class RepositoryTestBase : DatabaseAwareTestBase
{
    protected IReadOnlyCollection<User> Users = [];

    public User GetUser(int index) => GetUser(SeedDataFactory.Users.GetIdentifier(index));

    public User GetUser(string id) => Users.Single(u => u.Id == id);

    public override async ValueTask InitializeAsync()
    {
        // Lets start with 30 users
        Users = Enumerable.Range(1, 30).Select(SeedDataFactory.Users.Create).ToArray();
        await Context.Users.AddRangeAsync(Users, TestContext.Current.CancellationToken);

        // Two of the users can be organisers, each organising 60 games
        var games = Enumerable.Range(1, 120)
            .Select(i =>
            {
                var organiserId = i <= 60
                    ? SeedDataFactory.Users.GetIdentifier(5)
                    : SeedDataFactory.Users.GetIdentifier(8);

                return SeedDataFactory.Games.Create(i, Users.Single(u => u.Id == organiserId));
            })
            .ToArray();
        await Context.Games.AddRangeAsync(games, TestContext.Current.CancellationToken);

        // Fill the games with dummy players, organiser is always on the home team for the tests
        var players = games.SelectMany(game =>
        {
            var userPlayer = new Player(game, game.Organiser);
            userPlayer.AssignTeam(GameTeamEnum.Home, game.Organiser.Rating);
            var homeTeam = Enumerable.Range(100, game.TeamSize - 1).Select(i => SeedDataFactory.Players.CreateDummy(i, game, GameTeamEnum.Home)).ToArray();
            var awayTeam = Enumerable.Range(200, game.TeamSize).Select(i => SeedDataFactory.Players.CreateDummy(i, game, GameTeamEnum.Away)).ToArray();
            return homeTeam.Union([userPlayer]).Union(awayTeam);
        });

        await Context.Players.AddRangeAsync(players, TestContext.Current.CancellationToken);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}