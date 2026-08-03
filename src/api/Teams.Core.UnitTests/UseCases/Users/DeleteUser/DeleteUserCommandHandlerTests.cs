using Teams.Core.Exceptions;
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
            new(UnitOfWork, new FakeLogger<DeleteUserCommandHandler>());

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            UsersRepository.GetByIdAsync("missing-user", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = new DeleteUserCommand("missing-user");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(User), exception.ResourceType);
            Assert.Equal("missing-user", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldPersistDeletion_WhenUserExists()
        {
            var existingUser = CreateExistingUser();
            UsersRepository.GetByIdAsync(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
            var command = new DeleteUserCommand(existingUser.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.NotNull(result.DateDeleted);
            await UsersRepository.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}