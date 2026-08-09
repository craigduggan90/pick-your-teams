using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Invitations.GetInvitationById;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Invitations.GetInvitationById;

public static class GetInvitationByIdQueryHandlerTests
{
    public class HandleAsync : UseCaseTestBase<GetInvitationByIdQuery>
    {
        private GetInvitationByIdQueryHandler CreateSut() => new(InvitationsRepository, ActorAccessor);

        private static (Game Game, Invitation Invitation) CreateExisting(string? userId = "user-id")
        {
            var game = new Game("organiser-id", "location", DateTime.UtcNow, 60, 5);
            var invitation = new Invitation(game.Id, userId, "player@example.com") { Game = game };
            return (game, invitation);
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenInvitationDoesNotExist()
        {
            InvitationsRepository.GetByIdAsync("missing-invitation", Arg.Any<CancellationToken>()).Returns((Invitation?)null);
            var query = new GetInvitationByIdQuery("missing-invitation");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Invitation), exception.ResourceType);
            Assert.Equal("missing-invitation", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldReturnInvitation_WhenInvitationExists()
        {
            var (_, existingInvitation) = CreateExisting();
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("organiser-id", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingInvitation, result);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedException_WhenActorIsNeitherOrganiserNorTheInvitedUser()
        {
            var (_, existingInvitation) = CreateExisting();
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldReturnInvitation_WhenActorIsTheOrganiser()
        {
            var (_, existingInvitation) = CreateExisting();
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("organiser-id", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingInvitation, result);
        }

        [Fact]
        public async Task ShouldReturnInvitation_WhenActorIsTheInvitedUser()
        {
            var (_, existingInvitation) = CreateExisting();
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("user-id", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingInvitation, result);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedException_WhenInvitationHasNoLinkedUserAndActorIsNotOrganiser()
        {
            var (_, existingInvitation) = CreateExisting(userId: null);
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(query, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldReturnInvitation_WhenInvitationHasNoLinkedUserAndActorIsOrganiser()
        {
            var (_, existingInvitation) = CreateExisting(userId: null);
            InvitationsRepository.GetByIdAsync(existingInvitation.Id, Arg.Any<CancellationToken>()).Returns(existingInvitation);
            ActorAccessor.Current.Returns(new Actor("organiser-id", "tag", "display-name"));
            var query = new GetInvitationByIdQuery(existingInvitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(query, TestContext.Current.CancellationToken);

            Assert.Same(existingInvitation, result);
        }
    }
}