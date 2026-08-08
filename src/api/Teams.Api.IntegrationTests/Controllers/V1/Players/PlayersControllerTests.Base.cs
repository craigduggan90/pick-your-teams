using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Players;

public static partial class PlayersControllerTests
{
    private const string Url = "api/v1/players";
    private const string VersionlessUrl = "api/players";

    public abstract class PlayersControllerTestsBase(ApiWebApplicationFactory factory)
        : ApiControllerTestsBase(factory), IAsyncLifetime
    {
        protected static readonly DateTimeOffset BaseDate = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        protected User Organiser { get; } = EntityFactory.CreateUser(
            id: "organiser-001", displayName: "Test Organiser", dateCreated: BaseDate);

        protected Game SeedGame => field ??= EntityFactory.CreateGame(Organiser.Id, id: "game-001", dateCreated: BaseDate);

        /// <summary>
        /// 30 dummy players attached to <see cref="SeedGame"/> - enough variety for GetPlayers' filter and pagination
        /// tests. No user-linked players here: tests that need one (CreatePlayer duplicate checks, UserId/Type
        /// filters, DeletePlayer's self-authorised branch) seed their own dedicated user + player.
        /// </summary>
        protected IReadOnlyList<Player> SeedPlayers => field ??= Enumerable.Range(1, 30).Select(BuildSeedPlayer).ToList();

        public virtual async ValueTask InitializeAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // Children first: players reference games, games reference users.
            await context.Players.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Games.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Users.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);

            await context.Users.AddAsync(Organiser, TestContext.Current.CancellationToken);
            await context.Games.AddAsync(SeedGame, TestContext.Current.CancellationToken);
            await context.Players.AddRangeAsync(SeedPlayers, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private Player BuildSeedPlayer(int index) =>
            EntityFactory.CreatePlayer(
                SeedGame.Id,
                id: $"player-{index:D3}",
                displayName: $"Player {index:D3}",
                rating: 1000 + index, // 1001 - 1030
                team: (index % 3) switch
                {
                    0 => GameTeamEnum.Home,
                    1 => GameTeamEnum.Away,
                    _ => GameTeamEnum.None
                },
                dateCreated: BaseDate.AddDays(index));
    }
}