using Teams.Core.UseCases.Users.UpdateUser;

namespace Teams.Core.UnitTests.UseCases.Users.UpdateUser;

public static class UpdateUserCommandValidatorTests
{
    public class ValidateAsync
    {
        private static UpdateUserCommandValidator CreateSut() => new();

        private static UpdateUserCommand CreateValidCommand() =>
            new("user-id", "tag-value", "display-name", "user@example.com", "+15551234567");

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
            var command = CreateValidCommand() with { Tag = null, DisplayName = null, Email = null, Mobile = null };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenTagExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Tag = new string('a', 37) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.Tag));
        }

        [Fact]
        public async Task ShouldFail_WhenDisplayNameExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = new string('a', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.DisplayName));
        }

        [Fact]
        public async Task ShouldFail_WhenEmailIsNotValidFormat()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Email = "not-an-email" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.Email));
        }

        [Fact]
        public async Task ShouldFail_WhenMobileExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Mobile = new string('1', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.Mobile));
        }
    }
}