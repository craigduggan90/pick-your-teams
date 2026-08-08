using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    private const string Url = "api/v1/games";
    private const string VersionlessUrl = "api/games";

    public abstract class GamesControllerTestsBase(ApiWebApplicationFactory factory)
        : ApiControllerTestsBase(factory), IAsyncLifetime
    {
        protected static readonly DateTimeOffset BaseDate = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        /// <summary>A small, stable pool of users used as game organisers.</summary>
        protected IReadOnlyList<User> SeedOrganisers { get; } =
            Enumerable.Range(1, 5).Select(CreateSeedOrganiser).ToList();

        /// <summary>
        /// 30 games, cycling location/duration/team size/status against the organiser pool - enough variety for
        /// GetGames' filter and pagination tests. No players are attached here: tests that need players (SetTeams,
        /// GetTeams, RecordResult, etc.) seed their own dedicated game and players via <see cref="EntityFactory"/>,
        /// so those fixtures stay self-contained and easy to reason about rather than reverse-engineered from a
        /// shared pool.
        /// </summary>
        protected IReadOnlyList<Game> SeedGames => field ??= Enumerable.Range(1, 30).Select(BuildSeedGame).ToList();

        public virtual async ValueTask InitializeAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // Children first: players reference games, games reference users.
            await context.Players.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Games.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Users.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);

            await context.Users.AddRangeAsync(SeedOrganisers, TestContext.Current.CancellationToken);
            await context.Games.AddRangeAsync(SeedGames, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private Game BuildSeedGame(int index)
        {
            var organiser = SeedOrganisers[index % SeedOrganisers.Count];

            return EntityFactory.CreateGame(
                organiser.Id,
                id: $"game-{index:D3}",
                location: $"Test Venue {index:D3}",
                startTime: BaseDate.AddDays(index).UtcDateTime,
                duration: 30 + index, // 31 - 60
                teamSize: 3 + index % 9, // cycles 3 - 11
                dateCreated: BaseDate.AddDays(index),
                postCreationSteps: game =>
                {
                    // Half the seed games are finished, split between a Home and an Away win.
                    if (index % 2 == 0)
                        game.SetResult(index % 4 == 0 ? GameTeamEnum.Home : GameTeamEnum.Away);
                });
        }

        private static User CreateSeedOrganiser(int index) =>
            EntityFactory.CreateUser(
                id: $"organiser-{index:D3}",
                displayName: $"Test Organiser {index:D3}",
                externalId: $"external-organiser-{index:D3}",
                email: $"organiser{index:D3}@test.net",
                dateCreated: BaseDate.AddDays(index));

        /// <summary>Seeds a dedicated game with unassigned players, for tests that need to exercise team assignment.</summary>
        protected async Task<(Game Game, IReadOnlyList<Player> Players)> SeedGameWithUnassignedPlayersAsync(
            User organiser, int playerCount = 4, int teamSize = 3)
        {
            var game = EntityFactory.CreateGame(organiser.Id, teamSize: teamSize);
            var players = Enumerable.Range(1, playerCount)
                .Select(i => EntityFactory.CreatePlayer(game.Id, displayName: $"Player {i}", rating: 1000))
                .ToList();

            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await context.Players.AddRangeAsync(players, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return (game, players);
        }
    }
}