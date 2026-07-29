using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class ConcurrencyTokenMismatchExceptionTests
{
    private const string ExpectedMessage = "Concurrency Token does not match current record state.";

    public class Constructor
    {
        [Fact]
        public void SetsMessage_WhenConstructed()
        {
            var exception = new ConcurrencyTokenMismatchException();

            Assert.Equal(ExpectedMessage, exception.Message);
        }
    }

    public class ThrowIfMismatch
    {
        [Fact]
        public void DoesNotThrow_WhenValuesMatch()
        {
            var exception = Record.Exception(
                () => ConcurrencyTokenMismatchException.ThrowIfMismatch("abc123", "abc123"));

            Assert.Null(exception);
        }

        [Fact]
        public void DoesNotThrow_WhenValuesMatchWithDifferentCasing()
        {
            var exception = Record.Exception(
                () => ConcurrencyTokenMismatchException.ThrowIfMismatch("ABC123", "abc123"));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsConcurrencyTokenMismatchException_WhenValuesDiffer()
        {
            Assert.Throws<ConcurrencyTokenMismatchException>(
                () => ConcurrencyTokenMismatchException.ThrowIfMismatch("abc123", "def456"));
        }
    }
}