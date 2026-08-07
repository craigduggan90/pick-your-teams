using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Users.UpdateUser;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Users.UpdateUser;

public static class UpdateUserCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<UpdateUserCommand>
    {
        private static User CreateExistingUser(string id = "existing-user")
        {
            using var _ = new Teams.Common.Providers.Identifiers.IdentifierProviderContext(id);
            return new User("existing-display-name", "external-id", "existing@example.com", "+15551111111");
        }

        private UpdateUserCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, Validator, new FakeLogger<UpdateUserCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationExceptionAndNotLoadUser_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            ActorAccessor.Current.Returns(new Actor("existing-user", "tag", "display-name"));
            var command = new UpdateUserCommand("existing-user", null, null, null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotLoadUser_WhenActorIsNotSelf()
        {
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new UpdateUserCommand("existing-user", null, null, null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<AccessDeniedException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            UsersRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((User?)null);
            ActorAccessor.Current.Returns(new Actor("missing-user", "tag", "display-name"));
            var command = new UpdateUserCommand("missing-user", null, null, null, null);
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldNotCheckTagConflict_WhenTagNotProvided()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new UpdateUserCommand(existingUser.Id, null, "new-display-name", null, null);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await UsersRepository.DidNotReceive().GetByTagAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenTagIsTakenByAnotherUser()
        {
            var existingUser = CreateExistingUser("existing-user");
            var otherUser = CreateExistingUser("other-user");
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            UsersRepository.GetByTagAsync("taken-tag", Arg.Any<CancellationToken>()).Returns(otherUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new UpdateUserCommand(existingUser.Id, "taken-tag", null, null, null);
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotThrow_WhenTagAlreadyBelongsToSameUser()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            UsersRepository.GetByTagAsync(existingUser.Tag, Arg.Any<CancellationToken>()).Returns(existingUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new UpdateUserCommand(existingUser.Id, existingUser.Tag, null, null, null);
            var sut = CreateSut();

            var exception = await Record.ExceptionAsync(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldNotPersistChanges_WhenUpdateResultsInNoActualChange()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            UsersRepository.GetByTagAsync(existingUser.Tag, Arg.Any<CancellationToken>()).Returns(existingUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new UpdateUserCommand(
                existingUser.Id, existingUser.Tag, existingUser.DisplayName, existingUser.EmailAddress, existingUser.Mobile);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await UsersRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldPersistChangesAndReturnUpdatedUser_WhenUserIsDirtyAfterUpdate()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new UpdateUserCommand(existingUser.Id, null, "new-display-name", "new@example.com", "+15559998888");
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal("new-display-name", result.DisplayName);
            Assert.Equal("new@example.com", result.EmailAddress);
            Assert.Equal("+15559998888", result.Mobile);
            await UsersRepository.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}