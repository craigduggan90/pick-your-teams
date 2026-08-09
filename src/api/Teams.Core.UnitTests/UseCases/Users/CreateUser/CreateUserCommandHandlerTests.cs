using Teams.Core.Exceptions;
using Teams.Core.UseCases.Users.CreateUser;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Users.CreateUser;

public static class CreateUserCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CreateUserCommand>
    {
        private static readonly CreateUserCommand ValidCommand =
            new("display-name", "external-id", "user@example.com", "+15551234567");

        private CreateUserCommandHandler CreateSut() =>
            new(UnitOfWork, Validator, new FakeLogger<CreateUserCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(ValidCommand, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotCreateUser_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(ValidCommand, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCreateUser_WithRequestValues_WhenValidationSucceeds()
        {
            UsersRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<User>()!);
            var sut = CreateSut();

            var result = await sut.HandleAsync(ValidCommand, TestContext.Current.CancellationToken);

            Assert.Equal(ValidCommand.DisplayName, result.DisplayName);
            Assert.Equal(ValidCommand.ExternalId, result.ExternalId);
            Assert.Equal(ValidCommand.Email, result.EmailAddress);
            Assert.Equal(ValidCommand.Mobile, result.Mobile);
        }

        [Fact]
        public async Task ShouldReturnTheCreatedUser_WhenValidationSucceeds()
        {
            var created = new User(ValidCommand.DisplayName, ValidCommand.ExternalId, ValidCommand.Email, ValidCommand.Mobile);
            UsersRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(created);
            var sut = CreateSut();

            var result = await sut.HandleAsync(ValidCommand, TestContext.Current.CancellationToken);

            Assert.Same(created, result);
        }

        [Fact]
        public async Task ShouldSaveChanges_WhenValidationSucceeds()
        {
            UsersRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<User>()!);
            var sut = CreateSut();

            await sut.HandleAsync(ValidCommand, TestContext.Current.CancellationToken);

            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}