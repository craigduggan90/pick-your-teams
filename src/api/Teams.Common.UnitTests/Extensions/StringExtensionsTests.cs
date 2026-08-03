using Teams.Common.Extensions;

namespace Teams.Common.UnitTests.Extensions;

public static class StringExtensionsTests
{
    public class ToCamelCase
    {
        [Fact]
        public void Should_NotChange_FormattedString()
        {
            const string input = "thisIsInCamelCaseAlready";
            var actual = input.ToCamelCase();
            Assert.Equal(input, actual);
        }

        [Fact]
        public void Should_FormatString()
        {
            const string input = "ThisIsNotInCamelCase";
            const string expected = "thisIsNotInCamelCase";
            var actual = input.ToCamelCase();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToKebabCaseLower_ShouldNotChange_GivenStringInKebabCase()
        {
            const string input = "this-is-in-kebab-case-already";
            var actual = input.ToKebabCaseLower();
            Assert.Equal(input, actual);
        }

        [Fact]
        public void ToKebabCaseLower_ShouldConvertToKebabCase_GivenStringNotInKebabCase()
        {
            const string input = "thisIsNotInKebabCase";
            const string expected = "this-is-not-in-kebab-case";
            var actual = input.ToKebabCaseLower();
            Assert.Equal(expected, actual);
        }
    }
}