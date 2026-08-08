using System.Net;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;
using Teams.Domain.Enums;

namespace Teams.Api.IntegrationTests.Controllers.V1.Games;

public static partial class GamesControllerTests
{
    public class GetGames(ApiWebApplicationFactory factory) : GamesControllerTestsBase(factory)
    {
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
    }
}