using Teams.Core.UseCases.Games.UpdateGame;

namespace Teams.Core.UnitTests.UseCases.Games.UpdateGame;

public static class UpdateGameCommandValidatorTests
{
    public class ValidateAsync
    {
        private static UpdateGameCommandValidator CreateSut() => new();

        private static UpdateGameCommand CreateValidCommand() =>
            new("game-id", "location", DateTime.UtcNow, 60);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenAllOptionalFieldsAreNull()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Location = null, StartTime = null, Duration = null };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenLocationExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Location = new string('a', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateGameCommand.Location));
        }

        [Theory]
        [InlineData(15)]
        [InlineData(120)]
        public async Task ShouldSucceed_WhenDurationAtBoundary(int duration)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Duration = duration };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(14)]
        [InlineData(121)]
        public async Task ShouldFail_WhenDurationOutsideBoundary(int duration)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Duration = duration };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateGameCommand.Duration));
        }
    }
}