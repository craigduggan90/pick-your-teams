using Teams.Core.UseCases.Players.CreateDummyPlayer;

namespace Teams.Core.UnitTests.UseCases.Players.CreateDummyPlayer;

public static class CreateDummyPlayerCommandValidatorTests
{
    public class ValidateAsync
    {
        private static CreateDummyPlayerCommandValidator CreateSut() => new();

        private static CreateDummyPlayerCommand CreateValidCommand() =>
            new("game-id", "display-name", 1000);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenDisplayNameIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = "" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDummyPlayerCommand.DisplayName));
        }

        [Fact]
        public async Task ShouldFail_WhenDisplayNameExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = new string('a', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDummyPlayerCommand.DisplayName));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2000)]
        public async Task ShouldSucceed_WhenEstimatedRatingAtBoundary(int rating)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { EstimatedRating = rating };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2001)]
        public async Task ShouldFail_WhenEstimatedRatingOutsideBoundary(int rating)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { EstimatedRating = rating };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateDummyPlayerCommand.EstimatedRating));
        }
    }
}