using Teams.Core.Exceptions;
using Teams.Core.Models;
using Teams.Core.UseCases.Users.DeleteUser;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Users.DeleteUser;

public static class DeleteUserCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<DeleteUserCommand>
    {
        private static User CreateExistingUser() =>
            new("existing-display-name", "external-id", "existing@example.com", "+15551111111");

        private DeleteUserCommandHandler CreateSut() =>
            new(UnitOfWork, ActorAccessor, new FakeLogger<DeleteUserCommandHandler>());

        [Fact]
        public async Task ShouldThrowAccessDeniedExceptionAndNotLoadUser_WhenActorIsNotSelf()
        {
            ActorAccessor.Current.Returns(new Actor("some-other-actor", "tag", "display-name"));
            var command = new DeleteUserCommand("user-id");
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
            var command = new DeleteUserCommand("missing-user");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldPersistDeletionAndReturnTheUser_WhenActorIsSelf()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            ActorAccessor.Current.Returns(new Actor(existingUser.Id, existingUser.Tag, existingUser.DisplayName));
            var command = new DeleteUserCommand(existingUser.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(existingUser, result);
            Assert.NotNull(result.DateDeleted);
            await UsersRepository.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}