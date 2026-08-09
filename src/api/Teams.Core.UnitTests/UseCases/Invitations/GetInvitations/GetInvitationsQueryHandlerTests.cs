using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Invitations.GetInvitations;
using Teams.Data.Models;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Invitations.GetInvitations;

public static class GetInvitationsQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetInvitationsQuery>
    {
        private GetInvitationsQueryHandler CreateSut() => new(InvitationsRepository, GamesRepository, ActorAccessor);

        private static GetInvitationsQuery CreateQuery(string? userId = null, string? gameId = null) => new(
            GameId: gameId,
            UserId: userId,
            EmailAddress: null,
            Status: null,
            CreatedFrom: null,
            CreatedTo: null,
            ModifiedFrom: null,
            ModifiedTo: null,
            PageSize: null,
            Cursor: null);

        [Fact]
        public async Task ShouldForwardAllFilters_ToRepository()
        {
            var createdFrom = new DateTime(2026, 3, 1);
            var createdTo = new DateTime(2026, 4, 1);
            var modifiedFrom = new DateTime(2026, 5, 1);
            var modifiedTo = new DateTime(2026, 6, 1);
            const InvitationStatusEnum status = InvitationStatusEnum.Open;
            ActorAccessor.Current.Returns(new Actor("user-id", "tag", "display-name"));
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
            var query = CreateQuery();
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
            var query = CreateQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotQueryRepository_WhenUserIdProvidedAndActorIsNotThatUser()
        {
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var query = CreateQuery(userId: "user-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            await InvitationsRepository.DidNotReceive().GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotThrow_WhenUserIdProvidedAndActorIsThatUser()
        {
            ActorAccessor.Current.Returns(new Actor("user-id", "tag", "display-name"));
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = CreateQuery(userId: "user-id");
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldNotThrow_WhenNeitherUserIdNorGameIdIsProvided()
        {
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = CreateQuery();
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldNotThrow_WhenGameIdProvidedAndGameDoesNotExist()
        {
            // The game not existing means the invitations query will get no hits anyway - no need to block it.
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = CreateQuery(gameId: "missing-game");
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotQueryInvitations_WhenGameIdProvidedAndActorIsNotOrganiser()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var query = CreateQuery(gameId: game.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            await InvitationsRepository.DidNotReceive().GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldNotThrow_WhenGameIdProvidedAndActorIsOrganiser()
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("organiser-id", "tag", "display-name"));
            InvitationsRepository.GetInvitationsAsync(
                gameId: Arg.Any<string?>(),
                userId: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                status: Arg.Any<InvitationStatusEnum?>(),
                dateFilter: Arg.Any<DateFilter?>(),
                pagination: Arg.Any<PaginationFilter?>(),
                cancellationToken: Arg.Any<CancellationToken>()).Returns([]);
            var query = CreateQuery(gameId: game.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Empty(result);
        }
    }
}