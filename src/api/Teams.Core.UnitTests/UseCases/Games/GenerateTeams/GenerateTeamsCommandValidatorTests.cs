using Teams.Core.UseCases.Games.GenerateTeams;

namespace Teams.Core.UnitTests.UseCases.Games.GenerateTeams;

public static class GenerateTeamsCommandValidatorTests
{
    public class ValidateAsync
    {
        private static GenerateTeamsCommandValidator CreateSut() => new();

        private static GenerateTeamsCommand CreateValidQuery() =>
            new("game-id", [], [], 100, 3);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var query = CreateValidQuery();

            var result = await sut.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task ShouldSucceed_WhenCountAtBoundary(int count)
        {
            var sut = CreateSut();
            var query = CreateValidQuery() with { Count = count };

            var result = await sut.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task ShouldFail_WhenCountOutsideBoundary(int count)
        {
            var sut = CreateSut();
            var query = CreateValidQuery() with { Count = count };

            var result = await sut.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(GenerateTeamsCommand.Count));
        }

        [Fact]
        public async Task ShouldSucceed_WhenDifferentialAtBoundary()
        {
            var sut = CreateSut();
            var query = CreateValidQuery() with { Differential = 100 };

            var result = await sut.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenDifferentialBelowMinimum()
        {
            var sut = CreateSut();
            var query = CreateValidQuery() with { Differential = 99 };

            var result = await sut.ValidateAsync(query, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(GenerateTeamsCommand.Differential));
        }
    }
}