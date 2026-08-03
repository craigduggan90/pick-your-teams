using Teams.Core.UseCases.Games.CreateGame;

namespace Teams.Core.UnitTests.UseCases.Games.CreateGame;

public static class CreateGameCommandValidatorTests
{
    public class ValidateAsync
    {
        private static CreateGameCommandValidator CreateSut() => new();

        private static CreateGameCommand CreateValidCommand() =>
            new("organiser-id", "location", DateTime.UtcNow, 60, 5);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenLocationIsNull()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Location = null };

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
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGameCommand.Location));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(11)]
        public async Task ShouldSucceed_WhenTeamSizeAtBoundary(int teamSize)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { TeamSize = teamSize };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(12)]
        public async Task ShouldFail_WhenTeamSizeOutsideBoundary(int teamSize)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { TeamSize = teamSize };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGameCommand.TeamSize));
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
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGameCommand.Duration));
        }
    }
}