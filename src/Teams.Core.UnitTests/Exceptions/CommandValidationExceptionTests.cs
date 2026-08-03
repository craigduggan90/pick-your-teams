using FluentValidation.Results;
using Teams.Core.Exceptions;
using Teams.Domain.Entities;

namespace Teams.Core.UnitTests.Exceptions;

public static class CommandValidationExceptionTests
{
    private const string ExpectedMessage = "One or more validation errors occurred.";

    public class Constructor
    {
        [Fact]
        public void SetsErrors_WhenConstructed()
        {
            ValidationFailure[] errors = [new("PropertyName", "Error message")];

            var exception = new CommandValidationException(errors);

            Assert.Same(errors, exception.Errors);
        }

        [Fact]
        public void SetsMessage_WhenConstructed()
        {
            var exception = new CommandValidationException([]);

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsEmptyErrors_WhenConstructedWithEmptyCollection()
        {
            var exception = new CommandValidationException([]);

            Assert.Empty(exception.Errors);
        }
    }

    public class ThrowIfValidationFailed
    {
        [Fact]
        public void DoesNotThrow_WhenResultIsValid()
        {
            var result = new ValidationResult();

            var exception = Record.Exception(() => CommandValidationException.ThrowIfValidationFailed(result));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsCommandValidationException_WhenResultIsInvalid()
        {
            var result = new ValidationResult([new ValidationFailure("PropertyName", "Error message")]);

            Assert.Throws<CommandValidationException>(() => CommandValidationException.ThrowIfValidationFailed(result));
        }

        [Fact]
        public void ThrowsExceptionContainingOriginalErrors_WhenResultIsInvalid()
        {
            ValidationFailure[] failures = [new("PropertyName", "Error message")];
            var result = new ValidationResult(failures);

            var exception = Assert.Throws<CommandValidationException>(
                () => CommandValidationException.ThrowIfValidationFailed(result));

            Assert.Equivalent(failures, exception.Errors);
        }
    }

    public class ForTagConflict
    {
        [Fact]
        public void ReturnsCommandValidationException_WithSingleErrorOnTag()
        {
            var exception = CommandValidationException.ForTagConflict();

            var error = Assert.Single(exception.Errors);
            Assert.Equal(nameof(User.Tag), error.PropertyName);
            Assert.Equal("Tag not available.", error.ErrorMessage);
        }
    }
}