using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teams.Data.Context;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Invitations;

public static partial class InvitationsControllerTests
{
    private const string Url = "api/v1/invitations";
    private const string VersionlessUrl = "api/invitations";

    public abstract class InvitationsControllerTestsBase(ApiWebApplicationFactory factory)
        : ApiControllerTestsBase(factory), IAsyncLifetime
    {
        protected static readonly DateTimeOffset BaseDate = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        protected User Organiser { get; } = EntityFactory.CreateUser(
            id: "organiser-001", displayName: "Test Organiser", dateCreated: BaseDate);

        protected Game SeedGame => field ??= EntityFactory.CreateGame(Organiser.Id, id: "game-001", dateCreated: BaseDate);

        /// <summary>A small, stable pool of users to be invited - reused across seed invitations.</summary>
        protected IReadOnlyList<User> SeedInvitees { get; } = Enumerable.Range(1, 5)
            .Select(i => EntityFactory.CreateUser(
                id: $"invitee-{i:D3}", displayName: $"Invitee {i:D3}", dateCreated: BaseDate.AddDays(i)))
            .ToList();

        /// <summary>
        /// 30 invitations against <see cref="SeedGame"/>, cycling through every status and the invitee pool -
        /// enough variety for GetInvitations' filter and pagination tests. Action-specific tests (Accept, Decline,
        /// CreateInvitations) seed their own dedicated game/invitation, so those fixtures stay self-contained.
        /// </summary>
        protected IReadOnlyList<Invitation> SeedInvitations => field ??= Enumerable.Range(1, 30).Select(BuildSeedInvitation).ToList();

        public virtual async ValueTask InitializeAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            // Children first: invitations reference games and users, games reference users.
            await context.Invitations.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Games.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);
            await context.Users.IgnoreQueryFilters().ExecuteDeleteAsync(TestContext.Current.CancellationToken);

            await context.Users.AddAsync(Organiser, TestContext.Current.CancellationToken);
            await context.Users.AddRangeAsync(SeedInvitees, TestContext.Current.CancellationToken);
            await context.Games.AddAsync(SeedGame, TestContext.Current.CancellationToken);
            await context.Invitations.AddRangeAsync(SeedInvitations, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private Invitation BuildSeedInvitation(int index)
        {
            var invitee = SeedInvitees[index % SeedInvitees.Count];

            return EntityFactory.CreateInvitation(
                SeedGame.Id,
                invitee.Id,
                invitee.EmailAddress,
                id: $"invitation-{index:D3}",
                dateCreated: BaseDate.AddDays(index),
                postCreationSteps: invitation =>
                {
                    // Cycle deterministically through every status - roughly a quarter each.
                    switch (index % 4)
                    {
                        case 1: invitation.Accept(); break;
                        case 2: invitation.Decline(); break;
                        case 3: invitation.DispatchError("Delivery failed."); break;
                            // case 0: remains Open
                    }
                });
        }

        /// <summary>
        /// Seeds a dedicated game and a single Open invitation, isolated from the shared 30-invitation seed - for
        /// tests that need to mutate an invitation (Accept/Decline) without disturbing other tests in the same class.
        /// </summary>
        protected async Task<(Game Game, User Invitee, Invitation Invitation)> SeedOpenInvitationAsync(bool inviteeAlreadyInGame = false)
        {
            var organiser = EntityFactory.CreateUser(displayName: "Dedicated Organiser");
            var invitee = EntityFactory.CreateUser(displayName: "Dedicated Invitee");
            var game = EntityFactory.CreateGame(organiser.Id);
            var invitation = EntityFactory.CreateInvitation(game.Id, invitee.Id, invitee.EmailAddress);

            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Users.AddRangeAsync([organiser, invitee], TestContext.Current.CancellationToken);
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await context.Invitations.AddAsync(invitation, TestContext.Current.CancellationToken);

            if (inviteeAlreadyInGame)
            {
                var existingPlayer = EntityFactory.CreatePlayer(game.Id, userId: invitee.Id, type: PlayerTypeEnum.User);
                await context.Players.AddAsync(existingPlayer, TestContext.Current.CancellationToken);
            }

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            return (game, invitee, invitation);
        }
    }
}