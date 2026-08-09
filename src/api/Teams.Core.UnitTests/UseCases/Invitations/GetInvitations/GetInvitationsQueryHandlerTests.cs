using Teams.Core.UseCases.Invitations.GetInvitations;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Invitations.GetInvitations;

public static class GetInvitationsQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetInvitationsQuery>
    {
        private GetInvitationsQueryHandler CreateSut() => new(InvitationsRepository);

        [Fact]
        public async Task ShouldForwardAllFilters_ToRepository()
        {
            var createdFrom = new DateTime(2026, 3, 1);
            var createdTo = new DateTime(2026, 4, 1);
            var modifiedFrom = new DateTime(2026, 5, 1);
            var modifiedTo = new DateTime(2026, 6, 1);
            const InvitationStatusEnum status = InvitationStatusEnum.Open;
            var query = new GetInvitationsQuery(
                GameId: "game-id",
                UserId: "user-id",
                EmailAddress: "player@example.com",
                Status: status,
                CreatedFrom: createdFrom,
                CreatedTo: createdTo,
                ModifiedFrom: modifiedFrom,
                ModifiedTo: modifiedTo,
                PageSize: 10,
                Cursor: 42);
            var sut = CreateSut();

            await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            await InvitationsRepository.Received(1).GetInvitationsAsync(
                gameId: "game-id",
                userId: "user-id",
                emailAddress: "player@example.com",
                status: status,
                dateFilter: new DateFilter(
                    new RangeFilter<DateTime>(createdFrom, createdTo),
                    new RangeFilter<DateTime>(modifiedFrom, modifiedTo)),
                pagination: new PaginationFilter(42, 10),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnEntities_AsReadOnlyCollection()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            Invitation[] entities =
            [
                new(game.Id, "user-id-one", "one@example.com"),
                new(game.Id, "user-id-two", "two@example.com")
            ];
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns(entities);
            var query = new GetInvitationsQuery(
                GameId: null,
                UserId: null,
                EmailAddress: null,
                Status: null,
                CreatedFrom: null,
                CreatedTo: null,
                ModifiedFrom: null,
                ModifiedTo: null,
                PageSize: null,
                Cursor: null);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Equal(entities, result);
        }

        [Fact]
        public async Task ShouldReturnEmptyCollection_WhenNoEntitiesFound()
        {
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = new GetInvitationsQuery(
                GameId: null,
                UserId: null,
                EmailAddress: null,
                Status: null,
                CreatedFrom: null,
                CreatedTo: null,
                ModifiedFrom: null,
                ModifiedTo: null,
                PageSize: null,
                Cursor: null);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}