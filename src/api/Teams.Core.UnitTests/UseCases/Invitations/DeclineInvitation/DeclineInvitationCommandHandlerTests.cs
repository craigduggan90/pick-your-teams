using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.Services.Events;
using Teams.Core.UseCases.Invitations.DeclineInvitation;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UnitTests.UseCases.Invitations.DeclineInvitation;

public static class DeclineInvitationCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeclineInvitationCommand>
    {
        private static Game CreateGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static User CreateUser() =>
            new("display-name", $"external-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@test.io", null);

        private static Invitation CreateInvitation(Game game, User user) =>
            new(game.Id, user.Id, user.EmailAddress) { Game = game, User = user };

        private DeclineInvitationCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, EventPublisher, new FakeLogger<DeclineInvitationCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenInvitationDoesNotExist()
        {
            InvitationsRepository.GetByIdAsync("missing-invitation", Arg.Any<CancellationToken>()).Returns((Invitation?)null);
            var command = new DeclineInvitationCommand("missing-invitation");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Invitation), exception.ResourceType);
            Assert.Equal("missing-invitation", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotPersistChanges_WhenActorIsNotTheInvitedUser()
        {
            var game = CreateGame();
            var user = CreateUser();
            var invitation = CreateInvitation(game, user);
            InvitationsRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new DeclineInvitationCommand(invitation.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await InvitationsRepository.DidNotReceive().UpdateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldReturnInvitationUnchanged_WhenAlreadyDeclined()
        {
            var game = CreateGame();
            var user = CreateUser();
            var invitation = CreateInvitation(game, user);
            invitation.Decline();
            InvitationsRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new DeclineInvitationCommand(invitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(InvitationStatusEnum.Declined, result.Status);
            await InvitationsRepository.DidNotReceive().UpdateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
            await EventPublisher.DidNotReceive().PublishEventAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(InvitationStatusEnum.Accepted)]
        [InlineData(InvitationStatusEnum.Failed)]
        public async Task ShouldThrowRequestHandlerExceptionAndNotPersistChanges_WhenInvitationIsAlreadyResolved(
            InvitationStatusEnum status)
        {
            var game = CreateGame();
            var user = CreateUser();
            var invitation = CreateInvitation(game, user);
            if (status == InvitationStatusEnum.Accepted)
                invitation.Accept();
            else
                invitation.DispatchError("Delivery failed.");

            InvitationsRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new DeclineInvitationCommand(invitation.Id);
            var sut = CreateSut();

            await Assert.ThrowsAsync<RequestHandlerException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await InvitationsRepository.DidNotReceive().UpdateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldMarkInvitationAsFailed_WhenUserAlreadyHasPlayerInGame()
        {
            var game = CreateGame();
            var user = CreateUser();
            var existingPlayer = new Player(game, user);
            game.Players.Add(existingPlayer);
            var invitation = CreateInvitation(game, user);
            InvitationsRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new DeclineInvitationCommand(invitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(InvitationStatusEnum.Failed, result.Status);
            Assert.Equal("Unable to decline: player already in game.", result.ErrorMessage);
            await InvitationsRepository.Received(1).UpdateAsync(invitation, Arg.Any<CancellationToken>());
            await EventPublisher.DidNotReceive().PublishEventAsync(Arg.Any<IEvent>(), Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldDeclineInvitationAndPublishEvent_WhenOpen()
        {
            var game = CreateGame();
            var user = CreateUser();
            var invitation = CreateInvitation(game, user);
            InvitationsRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
            ActorAccessor.Current.Returns(new Actor(user.Id, user.Tag, user.DisplayName));
            var command = new DeclineInvitationCommand(invitation.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(InvitationStatusEnum.Declined, result.Status);
            await InvitationsRepository.Received(1).UpdateAsync(invitation, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await EventPublisher.Received(1).PublishEventAsync(
                Arg.Is<IEvent>(e => e is InvitationDeclinedEvent && ((InvitationDeclinedEvent)e).Id == invitation.Id),
                Arg.Any<CancellationToken>());
        }
    }
}