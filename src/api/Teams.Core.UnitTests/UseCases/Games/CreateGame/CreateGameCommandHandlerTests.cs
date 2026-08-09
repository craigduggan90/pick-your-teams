using Teams.Core.Exceptions;
using Teams.Core.UseCases.Games.CreateGame;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.UseCases.Games.CreateGame;

public static class CreateGameCommandHandlerTests
{
    public class HandleAsync : UseCaseTestBase<CreateGameCommand>
    {
        private static User CreateOrganiser() =>
            new("organiser-display-name", "external-id", "organiser@example.com", null);

        private static CreateGameCommand CreateValidCommand(string organiserId) =>
            new(organiserId, "location", DateTime.UtcNow, 60, 5);

        private CreateGameCommandHandler CreateSut() =>
            new(UnitOfWork, Validator, new FakeLogger<CreateGameCommandHandler>());

        [Fact]
        public async Task ShouldThrowCommandValidationException_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = CreateValidCommand("organiser-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ShouldNotLoadOrganiser_WhenValidationFails()
        {
            SetupValidator(InvalidResult());
            var command = CreateValidCommand("organiser-id");
            var sut = CreateSut();

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await UsersRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldThrowNotFoundException_WhenOrganiserDoesNotExist()
        {
            UsersRepository.GetByIdAsync("missing-organiser", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = CreateValidCommand("missing-organiser");
            var sut = CreateSut();

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            Assert.Equal(nameof(CreateGameCommand.OrganiserId), exception.ResourceType);
            Assert.Equal("missing-organiser", exception.ResourceIdentifier);
        }

        [Fact]
        public async Task ShouldNotCreateGame_WhenOrganiserDoesNotExist()
        {
            UsersRepository.GetByIdAsync("missing-organiser", Arg.Any<CancellationToken>()).Returns((User?)null);
            var command = CreateValidCommand("missing-organiser");
            var sut = CreateSut();

            await Assert.ThrowsAsync<NotFoundException>(
                () => sut.HandleAsync(command, TestContext.Current.CancellationToken));

            await GamesRepository.DidNotReceive().CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            await UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ShouldCreateGame_WithRequestValuesAndOrganiserId_WhenOrganiserExists()
        {
            var organiser = CreateOrganiser();
            UsersRepository.GetByIdAsync(organiser.Id, Arg.Any<CancellationToken>()).Returns(organiser);
            GamesRepository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<Game>()!);
            var command = CreateValidCommand(organiser.Id);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(organiser.Id, result.OrganiserId);
            Assert.Equal(command.Location, result.Location);
            Assert.Equal(command.StartTime, result.StartTime);
            Assert.Equal(command.Duration, result.Duration);
            Assert.Equal(command.TeamSize, result.TeamSize);
        }

        [Fact]
        public async Task ShouldReturnTheCreatedGame_WhenOrganiserExists()
        {
            var organiser = CreateOrganiser();
            UsersRepository.GetByIdAsync(organiser.Id, Arg.Any<CancellationToken>()).Returns(organiser);
            var command = CreateValidCommand(organiser.Id);
            var created = new Game(organiser.Id, command.Location, command.StartTime, command.Duration, command.TeamSize);
            GamesRepository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(created);
            var sut = CreateSut();

            var result = await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            Assert.Same(created, result);
        }

        [Fact]
        public async Task ShouldSaveChanges_WhenOrganiserExists()
        {
            var organiser = CreateOrganiser();
            UsersRepository.GetByIdAsync(organiser.Id, Arg.Any<CancellationToken>()).Returns(organiser);
            GamesRepository.CreateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<Game>()!);
            var command = CreateValidCommand(organiser.Id);
            var sut = CreateSut();

            await sut.HandleAsync(command, TestContext.Current.CancellationToken);

            await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}