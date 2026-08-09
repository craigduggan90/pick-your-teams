using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Common.Pagination;
using Teams.Data.Context;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Players;

public static partial class PlayersControllerTests
{
    public class GetPlayers(ApiWebApplicationFactory factory) : PlayersControllerTestsBase(factory)
    {
        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsUnsupported()
        {
            var request = CreateRequest(HttpMethod.Get, WithQuery(VersionlessUrl), apiVersion: "2.0");

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnBadRequest_WhenVersionIsNotProvided()
        {
            var request = CreateRequest(HttpMethod.Get, WithQuery(VersionlessUrl), apiVersion: null);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldReturnOk_WithDefaultPageSize_WhenNoFiltersProvided()
        {
            var request = CreateRequest(HttpMethod.Get, Url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(25, content.Data.Count); // 30 seed players, default page size 25
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByGameId_WhenGameIdProvided()
        {
            var otherOrganiser = EntityFactory.CreateUser(displayName: "Other Organiser");
            var otherGame = EntityFactory.CreateGame(otherOrganiser.Id);
            var outsider = EntityFactory.CreatePlayer(otherGame.Id, displayName: "Outsider");

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(otherOrganiser, TestContext.Current.CancellationToken);
                await context.Games.AddAsync(otherGame, TestContext.Current.CancellationToken);
                await context.Players.AddAsync(outsider, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var url = WithQuery(Url, ("GameId", SeedGame.Id), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(30, content.Data.Count);
            Assert.DoesNotContain(content.Data, p => p.Id == outsider.Id);
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByDisplayName_WhenDisplayNameProvided()
        {
            var existingPlayer = SeedPlayers[14];

            var url = WithQuery(Url, ("DisplayName", existingPlayer.GetDisplayName));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([existingPlayer.Id], content.Data.Select(p => p.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByUserId_WhenUserIdProvided()
        {
            var linkedUser = EntityFactory.CreateUser(displayName: "Linked User");
            var linkedPlayer = EntityFactory.CreatePlayer(
                SeedGame.Id, userId: linkedUser.Id, displayName: linkedUser.Tag, rating: linkedUser.Rating, type: PlayerTypeEnum.User);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(linkedUser, TestContext.Current.CancellationToken);
                await context.Players.AddAsync(linkedPlayer, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var url = WithQuery(Url, ("UserId", linkedUser.Id));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([linkedPlayer.Id], content.Data.Select(p => p.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByType_WhenTypeProvided()
        {
            var linkedUser = EntityFactory.CreateUser(displayName: "Linked User");
            var linkedPlayer = EntityFactory.CreatePlayer(
                SeedGame.Id, userId: linkedUser.Id, displayName: linkedUser.Tag, rating: linkedUser.Rating, type: PlayerTypeEnum.User);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                await context.Users.AddAsync(linkedUser, TestContext.Current.CancellationToken);
                await context.Players.AddAsync(linkedPlayer, TestContext.Current.CancellationToken);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var url = WithQuery(Url, ("Type", nameof(PlayerTypeEnum.User)));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal([linkedPlayer.Id], content.Data.Select(p => p.Id));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByRatingFrom_WhenRatingFromProvided()
        {
            var url = WithQuery(Url, ("RatingFrom", "1015"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: ratings 1015 - 1030 (players 15 - 30)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByRatingTo_WhenRatingToProvided()
        {
            var url = WithQuery(Url, ("RatingTo", "1015"), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: ratings 1001 - 1014 (players 1 - 14)
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByTeam_WhenTeamProvided()
        {
            var url = WithQuery(Url, ("Team", nameof(GameTeamEnum.Home)), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(10, content.Data.Count);
            Assert.All(content.Data, p => Assert.Equal(nameof(GameTeamEnum.Home), p.Team));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedFrom_WhenCreatedFromProvided()
        {
            var cutoff = SeedPlayers[14].DateCreated; // the 15th seeded player

            var url = WithQuery(Url, ("CreatedFrom", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(16, content.Data.Count); // inclusive: players 15 through 30
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCreatedTo_WhenCreatedToProvided()
        {
            var cutoff = SeedPlayers[14].DateCreated; // the 15th seeded player

            var url = WithQuery(Url, ("CreatedTo", cutoff.ToString("O")), ("PageSize", "100"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(14, content.Data.Count); // exclusive: players 1 through 14
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByCursor_WhenCursorProvided()
        {
            var firstPageUrl = WithQuery(Url, ("PageSize", "10"));
            var firstPageRequest = CreateRequest(HttpMethod.Get, firstPageUrl);
            var firstPageResponse = await Client.SendAsync(firstPageRequest, TestContext.Current.CancellationToken);
            var firstPage = await ReadContentAsync<PagedList<PlayerModel>>(firstPageResponse, TestContext.Current.CancellationToken);

            Assert.NotNull(firstPage);
            Assert.NotNull(firstPage.Cursor);

            var secondPageUrl = WithQuery(Url, ("PageSize", "10"), ("Cursor", firstPage.Cursor));
            var secondPageRequest = CreateRequest(HttpMethod.Get, secondPageUrl);
            var secondPageResponse = await Client.SendAsync(secondPageRequest, TestContext.Current.CancellationToken);
            var secondPage = await ReadContentAsync<PagedList<PlayerModel>>(secondPageResponse, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);
            Assert.NotNull(secondPage);
            Assert.Equal(10, secondPage.Data.Count);
            Assert.Empty(firstPage.Data.Select(p => p.Id).Intersect(secondPage.Data.Select(p => p.Id)));
        }

        [Fact]
        public async Task ShouldReturnOk_WithPagedList_FilteredByPageSize_WhenPageSizeProvided()
        {
            var url = WithQuery(Url, ("PageSize", "5"));
            var request = CreateRequest(HttpMethod.Get, url);

            var response = await Client.SendAsync(request, TestContext.Current.CancellationToken);
            var content = await ReadContentAsync<PagedList<PlayerModel>>(response, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.Equal(5, content.Data.Count);
        }
    }
}