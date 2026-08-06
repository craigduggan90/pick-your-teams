using Teams.Core.UseCases.Games.SetTeams;

namespace Teams.Core.UnitTests.UseCases.Games.SetTeams;

public static class SetTeamsCommandValidatorTests
{
    public class ValidateAsync
    {
        private static SetTeamsCommandValidator CreateSut() => new();

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = new SetTeamsCommand("game-id", ["p1", "p2"], ["p3", "p4"]);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenBothTeamsAreEmpty()
        {
            var sut = CreateSut();
            var command = new SetTeamsCommand("game-id", [], []);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WithDuplicatePlayerMessage_WhenHomeTeamHasDuplicate()
        {
            var sut = CreateSut();
            var command = new SetTeamsCommand("game-id", ["p1", "p1"], []);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == SetTeamsCommandValidator.DuplicatePlayer);
        }

        [Fact]
        public async Task ShouldFail_WithDuplicatePlayerMessage_WhenAwayTeamHasDuplicate()
        {
            var sut = CreateSut();
            var command = new SetTeamsCommand("game-id", [], ["p1", "p1"]);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == SetTeamsCommandValidator.DuplicatePlayer);
        }

        [Fact]
        public async Task ShouldFail_WithPlayerOnBothTeamsMessage_WhenPlayerAppearsInBothTeams()
        {
            var sut = CreateSut();
            var command = new SetTeamsCommand("game-id", ["p1"], ["p1"]);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == SetTeamsCommandValidator.PlayerOnBothTeams);
        }
    }
}