using Teams.Core.UseCases.Games.InvitePlayers;

namespace Teams.Core.UnitTests.UseCases.Games.InvitePlayers;

public static class InvitePlayersCommandValidatorTests
{
    public class ValidateAsync
    {
        private static InvitePlayersCommandValidator CreateSut() => new();

        private static InvitePlayersCommand CreateValidCommand(params string[] identifiers) =>
            new("game-id", identifiers);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand("tag-001", "user@example.com");

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenListIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(20)]
        public async Task ShouldSucceed_WhenListCountAtLimit(int count)
        {
            var sut = CreateSut();
            var identifiers = Enumerable.Range(1, count).Select(i => $"tag-{i:D3}").ToArray();
            var command = CreateValidCommand(identifiers);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenListExceedsLimit()
        {
            var sut = CreateSut();
            var identifiers = Enumerable.Range(1, 21).Select(i => $"tag-{i:D3}").ToArray();
            var command = CreateValidCommand(identifiers);

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(InvitePlayersCommand.UserIdentifiers));
        }

        [Fact]
        public async Task ShouldFail_WhenAnIdentifierIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand("tag-001", "");

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenAnIdentifierIsNeitherValidTagNorValidEmail()
        {
            var sut = CreateSut();
            var command = CreateValidCommand("tag-001", "-not-a-valid-tag-or-email");

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.ErrorMessage == "Value must represent either a valid tag or email address.");
        }

        [Fact]
        public async Task ShouldSucceed_WhenIdentifierIsValidTagOnly()
        {
            var sut = CreateSut();
            var command = CreateValidCommand("valid-tag-001");

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenIdentifierIsValidEmailOnly()
        {
            var sut = CreateSut();
            var command = CreateValidCommand("user@example.com");

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }
    }
}