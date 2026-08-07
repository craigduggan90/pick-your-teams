using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.Services.Invitations;
using Teams.Core.UseCases.Games.InvitePlayers;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Games.InvitePlayers;

public static class InvitePlayersCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<InvitePlayersCommand>
    {
        private readonly IGameInvitationDispatcher _inviter = Substitute.For<IGameInvitationDispatcher>();

        private static Game CreateExistingGame() => new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        private static User CreateExistingUser(string tag = "tag-001") =>
            new User("display-name", "external-id", "user@example.com", null) is var user
                ? UpdateTag(user, tag)
                : throw new InvalidOperationException();

        private static User UpdateTag(User user, string tag)
        {
            user.Update(tag, null, null, null);
            return user;
        }

        private InvitePlayersCommandHandler CreateSut() =>
            new(GamesRepository, UsersRepository, ActorAccessor, _inviter, Validator, new FakeLogger<InvitePlayersCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationExceptionAndNotLoadGame_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = new InvitePlayersCommand("game-id", []);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenGameDoesNotExist()
        {
            GamesRepository.GetByIdAsync("missing-game", Arg.Any<CancellationToken>()).Returns((Game?)null);
            var command = new InvitePlayersCommand("missing-game", []);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(Game), exception.ResourceType);
            Assert.Equal("missing-game", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndSendNoInvitations_WhenActorIsNotOrganiser()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new InvitePlayersCommand(game.Id, ["someone@example.com"]);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await _inviter.DidNotReceive().SendNewUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>());
            await _inviter.DidNotReceive().SendExistingUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldSendExistingUserInvitation_WhenIdentifierIsATagMatchingAUser()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser("existing-tag");
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByTagAsync("existing-tag", Arg.Any<CancellationToken>()).Returns(user);
            var command = new InvitePlayersCommand(game.Id, ["existing-tag"]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await _inviter.Received(1).SendExistingUserInvitationAsync(game, user.Id, user.EmailAddress);
            await _inviter.DidNotReceive().SendNewUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldSendExistingUserInvitation_WhenIdentifierIsAnEmailMatchingAUser()
        {
            var game = CreateExistingGame();
            var user = CreateExistingUser();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByEmailAddressAsync("existing@example.com", Arg.Any<CancellationToken>()).Returns(user);
            var command = new InvitePlayersCommand(game.Id, ["existing@example.com"]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await _inviter.Received(1).SendExistingUserInvitationAsync(game, user.Id, user.EmailAddress);
            await _inviter.DidNotReceive().SendNewUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldSendNewUserInvitation_WhenIdentifierIsAnEmailNotMatchingAnyUser()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByEmailAddressAsync("nobody@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new InvitePlayersCommand(game.Id, ["nobody@example.com"]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await _inviter.Received(1).SendNewUserInvitationAsync(game, "nobody@example.com");
            await _inviter.DidNotReceive().SendExistingUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldSendNoInvitation_WhenIdentifierIsATagNotMatchingAnyUser()
        {
            var game = CreateExistingGame();
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByTagAsync("missing-tag", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new InvitePlayersCommand(game.Id, ["missing-tag"]);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await _inviter.DidNotReceive().SendNewUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>());
            await _inviter.DidNotReceive().SendExistingUserInvitationAsync(Arg.Any<Game>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ShouldProcessEveryIdentifierIndependently_WhenMultipleProvided()
        {
            var game = CreateExistingGame();
            var existingByTag = CreateExistingUser("tag-a");
            var existingByEmail = CreateExistingUser("tag-b");
            GamesRepository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);
            UsersRepository.GetByTagAsync("tag-a", Arg.Any<CancellationToken>()).Returns(existingByTag);
            UsersRepository.GetByEmailAddressAsync("known@example.com", Arg.Any<CancellationToken>()).Returns(existingByEmail);
            UsersRepository.GetByEmailAddressAsync("new@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new InvitePlayersCommand(game.Id, ["tag-a", "known@example.com", "new@example.com"]);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(game, result);
            await _inviter.Received(1).SendExistingUserInvitationAsync(game, existingByTag.Id, existingByTag.EmailAddress);
            await _inviter.Received(1).SendExistingUserInvitationAsync(game, existingByEmail.Id, existingByEmail.EmailAddress);
            await _inviter.Received(1).SendNewUserInvitationAsync(game, "new@example.com");
        }
    }
}