using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.Services.Events;
using Teams.Core.UseCases.Invitations.CreateInvitations;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Invitations.CreateInvitations;

public static class CreateInvitationsCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CreateInvitationsCommand>
    {
        private static Game CreateExistingGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static User CreateUser(string tag)
        {
            var user = new User("display-name", $"external-{Guid.NewGuid():N}", $"{Guid.NewGuid():N}@test.io", null);
            user.Update(tag, null, null, null);
            return user;
        }

        private CreateInvitationsCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, EventPublisher, Validator, new FakeLogger<CreateInvitationsCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationExceptionAndNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new CreateInvitationsCommand("game-id", ["tag"]);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new CreateInvitationsCommand("missing-game", ["tag"]);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotCreateAnyInvitations_WhenActorIsNotOrganiser()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new CreateInvitationsCommand(game.Id, ["tag"]);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await InvitationsRepository.DidNotReceive().CreateAsync(Arg.Any<Invitation>(), Arg.Any<CancellationToken>());
            await UsersRepository.DidNotReceive().GetByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowCommandValidationExceptionAndNotPersistAnyChanges_WhenATagDoesNotMatchAUser()
        {
            var game = CreateExistingGame();
            var matchedUser = CreateUser("real-tag");
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByTagAsync("real-tag", Arg.Any<CancellationToken>()).Returns(matchedUser);
            UsersRepository.GetByTagAsync("fake-tag", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new CreateInvitationsCommand(game.Id, ["real-tag", "fake-tag"]);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Contains(exception.Errors, error => error.ErrorMessage.Contains("fake-tag"));
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
            await EventPublisher.DidNotReceive().PublishEventsAsync(Arg.Any<IEnumerable<IEvent>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCreateInvitationsAndPublishEvents_WhenAllTagsMatch()
        {
            var game = CreateExistingGame();
            var userOne = CreateUser("tag-one");
            var userTwo = CreateUser("tag-two");
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByTagAsync("tag-one", Arg.Any<CancellationToken>()).Returns(userOne);
            UsersRepository.GetByTagAsync("tag-two", Arg.Any<CancellationToken>()).Returns(userTwo);
            var command = new CreateInvitationsCommand(game.Id, ["tag-one", "tag-two"]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await InvitationsRepository.Received(1).CreateAsync(
                Arg.Is<Invitation>(i => i!.GameId == game.Id && i.UserId == userOne.Id), Arg.Any<CancellationToken>());
            await InvitationsRepository.Received(1).CreateAsync(
                Arg.Is<Invitation>(i => i!.GameId == game.Id && i.UserId == userTwo.Id), Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await EventPublisher.Received(1).PublishEventsAsync(
                Arg.Is<IEnumerable<IEvent>>(events => events != null && events.Count() == 2 &&
                    events.OfType<InvitationCreatedEvent>().Any(e => e.UserId == userOne.Id) &&
                    events.OfType<InvitationCreatedEvent>().Any(e => e.UserId == userTwo.Id)),
                Arg.Any<CancellationToken>());
        }
    }
}