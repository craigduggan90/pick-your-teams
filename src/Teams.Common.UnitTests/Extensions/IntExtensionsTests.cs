using System.Net;
using Teams.Common.Exceptions;
using Teams.Common.Extensions;

namespace Teams.Common.UnitTests.Extensions;

public static class IntExtensionsTests
{
    public class ToEnum
    {
        [Fact]
        public void ShouldReturnEnumValue_WhenValueDefined_ForNonNullableInput()
        {
            const HttpStatusCode expected = HttpStatusCode.HttpVersionNotSupported;
            var actual = 505.ToEnum<HttpStatusCode>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldReturnEnumValue_WhenValueDefined_ForNullableInput()
        {
            const HttpStatusCode expected = HttpStatusCode.GatewayTimeout;
            int? input = 504;
            var actual = input.ToEnum<HttpStatusCode>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldReturnNull_WhenInputValueNull()
        {
            int? input = null;
            var actual = input.ToEnum<HttpStatusCode>();
            Assert.Null(actual);
        }

        [Fact]
        public void ShouldThrowEnumConversionException_WhenValueNotDefined_ForNonNullableInput()
            => Assert.Throws<EnumConversionException>(() => 418.ToEnum<HttpStatusCode>());

        [Fact]
        public void ShouldThrowEnumConversionException_WhenValueNotDefined_ForNullableInput()
            => Assert.Throws<EnumConversionException>(() => ((int?)418).ToEnum<HttpStatusCode>());
    }
}