using System.Net;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Domain.Entities;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class SetTeams(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var existingGame = SeedGames[0];
            var organiser = SeedOrganisers.Single(u => u.Id == existingGame.OrganiserId);

            var request = CreateJsonRequest(
                HttpMethod.Put, $"{VersionlessUrl}/{existingGame.Id}/teams", new SetTeamsRequestModel([], []), apiVersion: "2.0");
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
                HttpMethod.Put, $"{VersionlessUrl}/{existingGame.Id}/teams", new SetTeamsRequestModel([], []), apiVersion: null);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenActorHeadersAreMissing()
        {
            var existingGame = SeedGames[0];

            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingGame.Id}/teams", new SetTeamsRequestModel([], []));

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

            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{existingGame.Id}/teams", new SetTeamsRequestModel([], []));
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

            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{id}/teams", new SetTeamsRequestModel([], []));
            WithActorHeaders(request, SeedOrganisers[0]);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(nameof(Game), problem.Extensions["resource"]?.ToString());
            Assert.Equal(id, problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenSamePlayerIsDuplicatedInATeam()
        {
            var organiser = SeedOrganisers[0];
            var (game, players) = await SeedGameWithUnassignedPlayersAsync(organiser);

            var invalidRequest = new SetTeamsRequestModel([players[0].Id, players[0].Id], []);
            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.NotEmpty(GetValidationErrors(problem, nameof(SetTeamsRequestModel.HomeTeamIds)));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenPlayerIsOnBothTeams()
        {
            var organiser = SeedOrganisers[0];
            var (game, players) = await SeedGameWithUnassignedPlayersAsync(organiser);

            var invalidRequest = new SetTeamsRequestModel([players[0].Id], [players[0].Id]);
            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Contains("Player assigned to both teams.", GetAllValidationErrors(problem));
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenGameIsFinished()
        {
            var finishedGame = SeedGames[1]; // seed game 2 - even index, already finished
            var organiser = SeedOrganisers.Single(u => u.Id == finishedGame.OrganiserId);

            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{finishedGame.Id}/teams", new SetTeamsRequestModel([], []));
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Teams cannot be changed for a completed game.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnUnprocessableEntity_WhenTooManyPlayersProvidedForHomeTeam()
        {
            var organiser = SeedOrganisers[0];
            var (game, players) = await SeedGameWithUnassignedPlayersAsync(organiser, playerCount: 5, teamSize: 3);

            var invalidRequest = new SetTeamsRequestModel(players.Take(4).Select(p => p.Id).ToArray(), []);
            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("Too many players provided for home team.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnNotFound_WhenPlayerIdDoesNotExist()
        {
            var organiser = SeedOrganisers[0];
            var (game, _) = await SeedGameWithUnassignedPlayersAsync(organiser);

            var invalidRequest = new SetTeamsRequestModel(["does-not-exist"], []);
            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", invalidRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("does-not-exist", problem.Extensions["identifier"]?.ToString());
        }

        [Fact]
        public async Task ShouldReturnNoContent_WithTeamsAssigned_WhenRequestIsValid()
        {
            var organiser = SeedOrganisers[0];
            var (game, players) = await SeedGameWithUnassignedPlayersAsync(organiser);

            var validRequest = new SetTeamsRequestModel([players[0].Id], [players[1].Id]);
            var request = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", validRequest);
            WithActorHeaders(request, organiser);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getRequest = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}/teams");
            var getResponse = await Client.SendAsync(getRequest, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<GameTeamsModel>(getResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(content);
            Assert.Equal([players[0].Id], content.Home!.Players.Select(p => p.Id));
            Assert.Equal([players[1].Id], content.Away!.Players.Select(p => p.Id));
        }

        [Fact]
        public async Task ShouldLeaveGameUnchanged_WhenRequestRepeatsTheExistingAssignment()
        {
            var organiser = SeedOrganisers[0];
            var (game, players) = await SeedGameWithUnassignedPlayersAsync(organiser);
            var requestModel = new SetTeamsRequestModel([players[0].Id], [players[1].Id]);

            var firstRequest = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", requestModel);
            WithActorHeaders(firstRequest, organiser);
            var firstResponse = await Client.SendAsync(firstRequest, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

            var firstGetRequest = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}");
            var firstGetResponse = await Client.SendAsync(firstGetRequest, TestContext.Current.CancellationToken);
            var firstDetail = await ReadContentAsync<GameDetailModel>(firstGetResponse, TestContext.Current.CancellationToken);
            Assert.NotNull(firstDetail);

            // Same request again - identical Home/Away assignment, nothing for the handler to actually change.
            var secondRequest = CreateJsonRequest(HttpMethod.Put, $"{Url}/{game.Id}/teams", requestModel);
            WithActorHeaders(secondRequest, organiser);
            var secondResponse = await Client.SendAsync(secondRequest, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);

            var secondGetRequest = CreateRequest(HttpMethod.Get, $"{Url}/{game.Id}");
            var secondGetResponse = await Client.SendAsync(secondGetRequest, TestContext.Current.CancellationToken);
            var secondDetail = await ReadContentAsync<GameDetailModel>(secondGetResponse, TestContext.Current.CancellationToken);
            Assert.NotNull(secondDetail);

            // The game itself wasn't touched the second time - Games.UpdateAsync was skipped since IsDirty stayed false.
            Assert.Equal(firstDetail.Modified, secondDetail.Modified);
        }
    }
}