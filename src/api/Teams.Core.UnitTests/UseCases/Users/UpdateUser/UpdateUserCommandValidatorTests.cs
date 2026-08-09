using Teams.Core.UseCases.Users.UpdateUser;

namespace Teams.Core.UnitTests.UseCases.Users.UpdateUser;

public static class UpdateUserCommandValidatorTests
{
    public class ValidateAsync
    {
        private static UpdateUserCommandValidator CreateSut() => new();

        private static UpdateUserCommand CreateValidCommand() =>
            new("user-id", "valid-tag", "valid-display-name", "user@example.com", "+15551234567");

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
        public async Task ShouldFail_WhenDisplayNameIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = "" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.DisplayName));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(101)]
        public async Task ShouldFail_WhenDisplayNameOutsideLengthBoundary(int length)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = new string('a', length) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.DisplayName));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(100)]
        public async Task ShouldSucceed_WhenDisplayNameAtLengthBoundary(int length)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = new string('a', length) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenTagIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Tag = "" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.Tag));
        }

        [Theory]
        [InlineData("ab")] // below minimum length
        [InlineData("-abc")] // starts with a dash
        [InlineData("__")] // no alphanumeric character
        [InlineData("has space")] // disallowed character
        public async Task ShouldFail_WhenTagIsInvalid(string tag)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Tag = tag };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateUserCommand.Tag));
        }

        [Theory]
        [InlineData("abc")] // at minimum length
        [InlineData("_ab")]
        [InlineData("ae_")]
        [InlineData("ae-")]
        [InlineData("ae.")]
        public async Task ShouldSucceed_WhenTagIsValid(string tag)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Tag = tag };

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
        public async Task ShouldSucceed_WhenTagAtMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Tag = new string('a', 36) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
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