using Teams.Core.UseCases.Games.RecordResult;

namespace Teams.Core.UnitTests.UseCases.Games.RecordResult;

public static class RecordGameResultCommandValidatorTests
{
    public class ValidateAsync
    {
        private static RecordGameResultCommandValidator CreateSut() => new();

        [Theory]
        [InlineData("Home")]
        [InlineData("Away")]
        [InlineData("None")]
        [InlineData("home")]
        public async Task ShouldSucceed_WhenWinnerIsValid(string winner)
        {
            var sut = CreateSut();
            var command = new RecordGameResultCommand("game-id", winner);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Draw")]
        [InlineData("not-a-team")]
        public async Task ShouldFail_WhenWinnerIsNotValid(string winner)
        {
            var sut = CreateSut();
            var command = new RecordGameResultCommand("game-id", winner);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(RecordGameResultCommand.Winner));
        }
    }
}