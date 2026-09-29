using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;
using Teams.Data.Context;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class GetGames(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
        private async Task SeedPlayerAsync(string gameId, string userId)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            var player = EntityFactory.CreatePlayer(gameId, userId: userId, type: PlayerTypeEnum.User);
            await context.Players.AddAsync(player, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var url = WithQuery(VersionlessUrl);
            var request = CreateRequest(HttpMethod.Get, url, apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var url = WithQuery(VersionlessUrl);
            var request = CreateRequest(HttpMethod.Get, url, apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithDefaultPageSize_WhenNoFiltersProvided()
        {
            var request = CreateRequest(HttpMethod.Get, Url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(25, content.Data.Count); // 30 seed games, default page size 25
            Assert.Equal(25, content.Count);

            Assert.All(content.Data, game =>
            {
                var existingGame = SeedGames.Single(g => g.Id == game.Id);
                var organiser = SeedOrganisers.Single(o => o.Id == existingGame.OrganiserId);
                Assert.Equal(organiser.Id, game.Organiser!.Id);
                Assert.Equal(organiser.Tag, game.Organiser.Tag);
                Assert.Equal(organiser.DisplayName, game.Organiser.DisplayName);
            });
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByLocation_WhenLocationProvided()
        {
            var existingGame = SeedGames[14];

            var url = WithQuery(Url, ("Location", existingGame.Location));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([existingGame.Id], content.Data.Select(g => g.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByStartTimeFrom_WhenStartTimeFromProvided()
        {
            var cutoff = SeedGames[14].StartTime; // the 15th seeded game

            var url = WithQuery(Url, ("StartTimeFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: games 15 through 30
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByStartTimeTo_WhenStartTimeToProvided()
        {
            var cutoff = SeedGames[14].StartTime; // the 15th seeded game

            var url = WithQuery(Url, ("StartTimeTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: games 1 through 14
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByDurationFrom_WhenDurationFromProvided()
        {
            // Duration = 30 + index, so 45 lands exactly between seed games 14 and 15.
            var url = WithQuery(Url, ("DurationFrom", "45"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: durations 45 - 60 (games 15 - 30)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByDurationTo_WhenDurationToProvided()
        {
            var url = WithQuery(Url, ("DurationTo", "45"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: durations 31 - 44 (games 1 - 14)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByTeamSize_WhenTeamSizeProvided()
        {
            // TeamSize cycles 3 - 11 as (3 + index % 9); index % 9 == 2 lands on TeamSize 5 for games 2, 11, 20, 29.
            var url = WithQuery(Url, ("TeamSize", "5"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(4, content.Data.Count);
            Assert.All(content.Data, game => Assert.Equal(5, game.TeamSize));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByStatus_WhenStatusProvided()
        {
            // Every even-indexed seed game (15 of 30) was finished.
            var url = WithQuery(Url, ("Status", nameof(GameStatusEnum.Finished)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(15, content.Data.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedFrom_WhenCreatedFromProvided()
        {
            var cutoff = SeedGames[14].DateCreated; // the 15th seeded game

            var url = WithQuery(Url, ("CreatedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: games 15 through 30
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedTo_WhenCreatedToProvided()
        {
            var cutoff = SeedGames[14].DateCreated; // the 15th seeded game

            var url = WithQuery(Url, ("CreatedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: games 1 through 14
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedFrom_WhenModifiedFromProvided()
        {
            // Seed data sets DateModified equal to DateCreated for every game (SetResult runs inside the same fixed
            // date-provider scope), so this mirrors the CreatedFrom test.
            var cutoff = SeedGames[14].DateCreated;

            var url = WithQuery(Url, ("ModifiedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByModifiedTo_WhenModifiedToProvided()
        {
            var cutoff = SeedGames[14].DateCreated;

            var url = WithQuery(Url, ("ModifiedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCursor_WhenCursorProvided()
        {
            var firstPageUrl = WithQuery(Url, ("PageSize", "10"));
            var firstPageRequest = CreateRequest(HttpMethod.Get, firstPageUrl);
            var firstPageResponse = await Client.SendAsync(firstPageRequest, TestContext.Current.CancellationToken);
            var firstPage = await ReadContentAsync<PagedList<GameModel>>(firstPageResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(firstPage);
            Assert.NotNull(firstPage.Cursor);

            var secondPageUrl = WithQuery(Url, ("PageSize", "10"), ("Cursor", firstPage.Cursor));
            var secondPageRequest = CreateRequest(HttpMethod.Get, secondPageUrl);
            var secondPageResponse = await Client.SendAsync(secondPageRequest, TestContext.Current.CancellationToken);
            var secondPage = await ReadContentAsync<PagedList<GameModel>>(secondPageResponse, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
            Assert.NotNull(secondPage);
            Assert.Equal(10, secondPage.Data.Count);
            Assert.Empty(firstPage.Data.Select(g => g.Id).Intersect(secondPage.Data.Select(g => g.Id)));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByPageSize_WhenPageSizeProvided()
        {
            var url = WithQuery(Url, ("PageSize", "5"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(5, content.Data.Count);
            Assert.Equal(5, content.Count);
        }

        [Fact]
        public async Task ShouldReturnPreconditionRequired_WhenOwnershipProvidedWithoutActorHeaders()
        {
            var url = WithQuery(Url, ("Ownership", nameof(GameOwnershipEnum.Both)));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var problem = await ReadProblemDetailsAsync(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.PreconditionRequired, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal("'Teams-User-Id' header value is required.", problem.Detail);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByOwnership_WhenOrganising()
        {
            var actor = SeedOrganisers[0];
            // organiser-000 organises every 5th seeded game (index % 5 == 0): games 0, 5, 10, 15, 20, 25.
            var organisedGameIds = SeedGames.Where(g => g.OrganiserId == actor.Id).Select(g => g.Id).ToArray();
            // Also a player (not organiser) in someone else's game - Organising must not include this one.
            var playedOnlyGame = SeedGames.First(g => g.OrganiserId != actor.Id);
            await SeedPlayerAsync(playedOnlyGame.Id, actor.Id);

            var url = WithQuery(Url, ("Ownership", nameof(GameOwnershipEnum.Organising)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, actor);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(organisedGameIds.OrderBy(id => id), content.Data.Select(g => g.Id).OrderBy(id => id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByOwnership_WhenPlaying()
        {
            var actor = SeedOrganisers[0];
            var organisedGame = SeedGames.First(g => g.OrganiserId == actor.Id);
            var playedGame = SeedGames.First(g => g.OrganiserId != actor.Id);
            await SeedPlayerAsync(playedGame.Id, actor.Id);

            var url = WithQuery(Url, ("Ownership", nameof(GameOwnershipEnum.Playing)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, actor);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([playedGame.Id], content.Data.Select(g => g.Id));
            Assert.DoesNotContain(organisedGame.Id, content.Data.Select(g => g.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByOwnership_WhenBoth()
        {
            var actor = SeedOrganisers[0];
            var organisedGameIds = SeedGames.Where(g => g.OrganiserId == actor.Id).Select(g => g.Id).ToArray();
            var playedGame = SeedGames.First(g => g.OrganiserId != actor.Id);
            await SeedPlayerAsync(playedGame.Id, actor.Id);
            var expectedIds = organisedGameIds.Append(playedGame.Id).Distinct().OrderBy(id => id);

            var url = WithQuery(Url, ("Ownership", nameof(GameOwnershipEnum.Both)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);
            WithActorHeaders(request, actor);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<GameModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(expectedIds, content.Data.Select(g => g.Id).OrderBy(id => id));
        }
    }
}