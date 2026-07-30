using System.Globalization;
using System.Text;
using Teams.Common.Pagination;

namespace Teams.Common.UnitTests.Pagination;

public static class CursorConverterTests
{
    private static string Encode(long value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));

    public class TryDecodeCursor
    {
        [Fact]
        public void ReturnsTrue_AndNullCursor_WhenInputIsNull()
        {
            string? input = null;

            var result = input.TryDecodeCursor(out var cursor);

            Assert.True(result);
            Assert.Null(cursor);
        }

        [Fact]
        public void ReturnsTrue_AndDecodedValue_WhenInputIsValidEncodedNumber()
        {
            var input = Encode(12345L);

            var result = input.TryDecodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(12345L, cursor);
        }

        [Fact]
        public void ReturnsFalse_WhenInputIsNotValidBase64()
        {
            const string input = "not-valid-base64!!!";

            var result = input.TryDecodeCursor(out var cursor);

            Assert.False(result);
            Assert.Null(cursor);
        }

        [Fact]
        public void ReturnsFalse_WhenDecodedStringIsNotANumber()
        {
            var input = Convert.ToBase64String(Encoding.UTF8.GetBytes("not-a-number"));

            var result = input.TryDecodeCursor(out var cursor);

            Assert.False(result);
            Assert.Null(cursor);
        }

        [Fact]
        public void ReturnsTrue_AndDecodedValue_WhenInputIsNegativeNumber()
        {
            var input = Encode(-500L);

            var result = input.TryDecodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(-500L, cursor);
        }

        [Fact]
        public void ReturnsTrue_AndDecodedValue_WhenInputIsZero()
        {
            var input = Encode(0L);

            var result = input.TryDecodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(0L, cursor);
        }

        [Fact]
        public void ReturnsFalse_WhenInputIsEmptyString()
        {
            const string input = "";

            var result = input.TryDecodeCursor(out var cursor);

            Assert.False(result);
            Assert.Null(cursor);
        }
    }

    public class TryEncodeCursor
    {
        [Fact]
        public void ReturnsTrue_AndNullCursor_WhenInputIsNull()
        {
            long? input = null;

            var result = input.TryEncodeCursor(out var cursor);

            Assert.True(result);
            Assert.Null(cursor);
        }

        [Fact]
        public void ReturnsTrue_AndEncodedValue_WhenInputHasValue()
        {
            long? input = 12345L;

            var result = input.TryEncodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(Encode(12345L), cursor);
        }

        [Fact]
        public void ReturnsTrue_AndEncodedValue_WhenInputIsNegative()
        {
            long? input = -500L;

            var result = input.TryEncodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(Encode(-500L), cursor);
        }

        [Fact]
        public void ReturnsTrue_AndEncodedValue_WhenInputIsZero()
        {
            long? input = 0L;

            var result = input.TryEncodeCursor(out var cursor);

            Assert.True(result);
            Assert.Equal(Encode(0L), cursor);
        }
    }

    public class RoundTrip
    {
        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(-1L)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void DecodesToOriginalValue_WhenEncoded(long value)
        {
            var encoded = ((long?)value).TryEncodeCursor(out var cursor);
            Assert.True(encoded);

            var decoded = cursor.TryDecodeCursor(out var result);

            Assert.True(decoded);
            Assert.Equal(value, result);
        }
    }
}