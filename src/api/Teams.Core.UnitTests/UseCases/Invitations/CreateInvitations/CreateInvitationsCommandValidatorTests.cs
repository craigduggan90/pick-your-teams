using Teams.Core.UseCases.Invitations.CreateInvitations;

namespace Teams.Core.UnitTests.UseCases.Invitations.CreateInvitations;

public static class CreateInvitationsCommandValidatorTests
{
    public class ValidateAsync
    {
        private static CreateInvitationsCommandValidator CreateSut() => new();

        private static CreateInvitationsCommand CreateValidCommand() =>
            new("game-id", ["valid-tag-1", "valid-tag-2"]);

        [Fact]
        public async Task ShouldSucceed_WhenRequestValid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task ShouldFail_WhenUserTagsIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = [] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateInvitationsCommand.UserTags));
        }

        [Fact]
        public async Task ShouldFail_WhenUserTagsContainsDuplicates()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = ["same-tag", "SAME-TAG"] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error =>
                error.PropertyName == nameof(CreateInvitationsCommand.UserTags) &&
                error.ErrorMessage == "Duplicate tags provided.");
        }

        [Fact]
        public async Task ShouldNotFailWithDuplicateError_WhenAllTagsAreUnique()
        {
            var sut = CreateSut();
            var command = CreateValidCommand();

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.DoesNotContain(result.Errors, error => error.ErrorMessage == "Duplicate tags provided.");
        }

        [Fact]
        public async Task ShouldFail_WhenATagIsEmpty()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = ["valid-tag", ""] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(CreateInvitationsCommand.UserTags)));
        }

        [Theory]
        [InlineData("ab")] // below the 3 character minimum
        [InlineData(".starts-with-a-dot")]
        [InlineData("has spaces")]
        public async Task ShouldFail_WhenATagIsNotValid(string tag)
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = ["valid-tag", tag] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(nameof(CreateInvitationsCommand.UserTags)));
        }

        [Fact]
        public async Task ShouldFail_WhenATagExceedsMaximumLength()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = ["valid-tag", new string('a', 37)] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ShouldReportOneErrorPerBadTag_WhenMultipleTagsAreInvalid()
        {
            var sut = CreateSut();
            var command = CreateValidCommand() with { UserTags = ["valid-tag", "ab", "also bad"] };

            var result = await sut.ValidateAsync(command, TestContext.Current.CancellationToken);

            Assert.Equal(
                2, result.Errors.Count(error => error.PropertyName.StartsWith(nameof(CreateInvitationsCommand.UserTags))));
        }
    }
}