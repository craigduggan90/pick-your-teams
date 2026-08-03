using Teams.Core.UseCases.Users.CreateUser;

namespace Teams.Core.UnitTests.UseCases.Users.CreateUser;

public static class CreateUserCommandValidatorTests
{
    public class ValidateAsync
    {
        private static CreateUserCommandValidator CreateSut() => new();

        private static CreateUserCommand CreateValidCommand() =>
            new("display-name", "external-id", "user@example.com", "+15551234567");

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldSucceed_WhenMobileIsNull()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Mobile = null };

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
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.DisplayName));
        }

        [Fact]
        public async Task ShouldFail_WhenDisplayNameExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { DisplayName = new string('a', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.DisplayName));
        }

        [Fact]
        public async Task ShouldFail_WhenExternalIdIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { ExternalId = "" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.ExternalId));
        }

        [Fact]
        public async Task ShouldFail_WhenExternalIdExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { ExternalId = new string('a', 256) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.ExternalId));
        }

        [Fact]
        public async Task ShouldFail_WhenEmailIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Email = "" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Email));
        }

        [Fact]
        public async Task ShouldFail_WhenEmailIsNotValidFormat()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Email = "not-an-email" };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Email));
        }

        [Fact]
        public async Task ShouldFail_WhenMobileExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { Mobile = new string('1', 101) };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Mobile));
        }
    }
}