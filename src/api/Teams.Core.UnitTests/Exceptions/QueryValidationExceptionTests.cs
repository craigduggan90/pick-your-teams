using FluentValidation.Results;
using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class QueryValidationExceptionTests
{
    private const string ExpectedMessage = "One or more validation errors occurred.";

    public class Constructor
    {
        [Fact]
        public void SetsErrors_WhenConstructed()
        {
            ValidationFailure[] errors = [new("PropertyName", "Error message")];

            var exception = new QueryValidationException(errors);

            Assert.Same(errors, exception.Errors);
        }

        [Fact]
        public void SetsMessage_WhenConstructed()
        {
            var exception = new QueryValidationException([]);

            Assert.Equal(ExpectedMessage, exception.Message);
        }

        [Fact]
        public void SetsEmptyErrors_WhenConstructedWithEmptyCollection()
        {
            var exception = new QueryValidationException([]);

            Assert.Empty(exception.Errors);
        }
    }

    public class ThrowIfValidationFailed
    {
        [Fact]
        public void DoesNotThrow_WhenResultIsValid()
        {
            var result = new ValidationResult();

            var exception = Record.Exception(() => QueryValidationException.ThrowIfValidationFailed(result));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsQueryValidationException_WhenResultIsInvalid()
        {
            var result = new ValidationResult([new ValidationFailure("PropertyName", "Error message")]);

            Assert.Throws<QueryValidationException>(() => QueryValidationException.ThrowIfValidationFailed(result));
        }

        [Fact]
        public void ThrowsExceptionContainingOriginalErrors_WhenResultIsInvalid()
        {
            ValidationFailure[] failures = [new("PropertyName", "Error message")];
            var result = new ValidationResult(failures);

            var exception = Assert.Throws<QueryValidationException>(
                () => QueryValidationException.ThrowIfValidationFailed(result));

            Assert.Equivalent(failures, exception.Errors);
        }
    }
}