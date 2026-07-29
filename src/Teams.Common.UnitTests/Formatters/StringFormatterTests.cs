using Teams.Common.Formatters;

namespace Teams.Common.UnitTests.Formatters;

public static class StringFormatterTests
{
    public class CamelCaseFormatter
    {
        [Fact]
        public void Should_NotChange_FormattedString()
        {
            const string input = "thisIsInCamelCaseAlready";
            var actual = StringFormatters.CamelCaseFormatter(input);
            Assert.Equal(input, actual);
        }

        [Fact]
        public void Should_FormatString()
        {
            const string input = "ThisIsNotInCamelCase";
            const string expected = "thisIsNotInCamelCase";
            var actual = StringFormatters.CamelCaseFormatter(input);
            Assert.Equal(expected, actual);
        }
    }
}