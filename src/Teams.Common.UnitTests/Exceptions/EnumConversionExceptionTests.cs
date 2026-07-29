using Teams.Common.Exceptions;
using System.Net;

namespace Teams.Common.UnitTests.Exceptions;

public static class EnumConversionExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitializeException_WithDefaultMessage()
        {
            var actual = new EnumConversionException();
            Assert.Equal(EnumConversionException.DefaultMessage, actual.Message);
        }
    }

    public class ForUndefinedValue
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            var enumType = typeof(HttpStatusCode);
            const byte value = 2;

            var expected = $"No HttpStatusCode found with value {value}.";
            var actual = EnumConversionException.ForUndefinedValue(enumType, value);
            Assert.Equal(expected, actual.Message);
        }
    }
}