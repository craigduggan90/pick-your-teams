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
        // Let's start with 30 users
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

        // Every game also gets a genuine non-organiser participant on Away, cycling through the
        // remaining 28 users - pulled from a pool that structurally excludes both organisers, so
        // there's no way for a game's organiser and its away participant to collide.
        var organiserIds = new[] { SeedDataFactory.Users.GetIdentifier(5), SeedDataFactory.Users.GetIdentifier(8) };
        var nonOrganiserUsers = Users.Where(u => !organiserIds.Contains(u.Id)).ToArray();

        var counter = 0;
        var players = games.SelectMany(game =>
        {
            var userPlayer = new Player(game, game.Organiser!);
            userPlayer.AssignTeam(GameTeamEnum.Home, game.Organiser!.Rating);

            var awayUser = nonOrganiserUsers[counter++ % nonOrganiserUsers.Length];
            var awayPlayer = new Player(game, awayUser);
            awayPlayer.AssignTeam(GameTeamEnum.Away, awayPlayer.Rating);

            var homeTeam = Enumerable.Range(100, game.TeamSize - 1).Select(i => SeedDataFactory.Players.CreateDummy(i, game, GameTeamEnum.Home)).ToArray();
            var awayTeam = Enumerable.Range(200, game.TeamSize - 1).Select(i => SeedDataFactory.Players.CreateDummy(i, game, GameTeamEnum.Away)).ToArray();
            return homeTeam.Union([userPlayer]).Union([awayPlayer]).Union(awayTeam);
        });

        await Context.Players.AddRangeAsync(players, TestContext.Current.CancellationToken);

        // Every game also gets one invitation, cycling deterministically through every status. Roughly a third are
        // tied to a real existing user (invited by tag); the rest are new-user invites (invited by email only).
        var invitations = games.Select((game, i) =>
        {
            var index = i + 1;
            var invitee = index % 3 == 0 ? nonOrganiserUsers[index % nonOrganiserUsers.Length] : null;
            return SeedDataFactory.Invitations.Create(index, game, invitee);
        });
        await Context.Invitations.AddRangeAsync(invitations, TestContext.Current.CancellationToken);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}