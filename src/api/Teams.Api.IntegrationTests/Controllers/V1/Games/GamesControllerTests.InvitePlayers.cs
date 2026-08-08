using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.IntegrationTests.TestServices;
using Teams.Core.Services.Invitations;
using Teams.Data.Context;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class InvitePlayers(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        // Requires TestGameInvitationDispatcher to be registered as a singleton, so the instance the live request
        // dispatches through is the same one this resolves afterward.
        private TestGameInvitationDispatcher Dispatcher =>
            (TestGameInvitationDispatcher)Factory.Services.GetRequiredService<IGameInvitationDispatcher>();

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{existingGame.Id}/invite", new InvitePlayersRequestModel(["test@test.net"]), apiVersion: "2.0");
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Post, $"{VersionlessUrl}/{existingGame.Id}/invite", new InvitePlayersRequestModel(["test@test.net"]), apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingGame = SeedGames[0];

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/invite", new InvitePlayersRequestModel(["test@test.net"]));

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnForbidden_WhenActorIsNotOrganiser()
        {
            var existingGame = SeedGames[0];
            var nonOrganiser = SeedOrganisers.First(u => u.Id != existingGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/invite", new InvitePlayersRequestModel(["test@test.net"]));
            WithActorHeaders(request, nonOrganiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Action only available to game organiser.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            const string id = "does-not-exist";

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{id}/invite", new InvitePlayersRequestModel(["test@test.net"]));
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTooManyIdentifiersProvided()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);
            var identifiers = Enumerable.Range(1, 21).Select(i => $"user{i}@test.net").ToArray();

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/invite", new InvitePlayersRequestModel(identifiers));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(InvitePlayersRequestModel.UserIdentifiers)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenIdentifierIsEmpty()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/invite", new InvitePlayersRequestModel([""]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetAllValidationErrors(problem));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenIdentifierIsNeitherTagNorEmail()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{existingGame.Id}/invite", new InvitePlayersRequestModel(["!!!not valid!!!"]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains("Value must represent either a valid tag or email address.", GetAllValidationErrors(problem));
        }

        [Fact]
        public async Task ShouldDispatchNewUserInvitation_WhenIdentifierIsAnUnregisteredEmail()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id);
            var email = $"{Guid.NewGuid():N}@test.net";
            await SeedGameAsync(game);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/invite", new InvitePlayersRequestModel([email]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var invitation = Assert.Single(Dispatcher.Invitations, i => i.GameId == game.Id);
            Assert.Equal(email, invitation.EmailAddress);
            Assert.Null(invitation.UserId);
        }

        [Fact]
        public async Task ShouldDispatchExistingUserInvitation_WhenIdentifierIsARegisteredTag()
        {
            var organiser = SeedOrganisers[0];
            var invitee = SeedOrganisers[1];
            var game = EntityFactory.CreateGame(organiser.Id);
            await SeedGameAsync(game);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/invite", new InvitePlayersRequestModel([invitee.Tag]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var invitation = Assert.Single(Dispatcher.Invitations, i => i.GameId == game.Id);
            Assert.Equal(invitee.Id, invitation.UserId);
            Assert.Equal(invitee.EmailAddress, invitation.EmailAddress);
        }

        [Fact]
        public async Task ShouldDispatchExistingUserInvitation_WhenIdentifierIsARegisteredEmail()
        {
            var organiser = SeedOrganisers[0];
            var invitee = SeedOrganisers[1];
            var game = EntityFactory.CreateGame(organiser.Id);
            await SeedGameAsync(game);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/invite", new InvitePlayersRequestModel([invitee.EmailAddress]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var invitation = Assert.Single(Dispatcher.Invitations, i => i.GameId == game.Id);
            Assert.Equal(invitee.Id, invitation.UserId);
        }

        [Fact]
        public async Task ShouldNotDispatchAnyInvitation_WhenTagDoesNotMatchAnyUser()
        {
            var organiser = SeedOrganisers[0];
            var game = EntityFactory.CreateGame(organiser.Id);
            await SeedGameAsync(game);

            var request = CreateJsonRequest(HttpMethod.Post, $"{Url}/{game.Id}/invite", new InvitePlayersRequestModel(["unknown-tag"]));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.DoesNotContain(Dispatcher.Invitations, i => i.GameId == game.Id);
        }

        private async Task SeedGameAsync(Game game)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await context.Games.AddAsync(game, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }
}