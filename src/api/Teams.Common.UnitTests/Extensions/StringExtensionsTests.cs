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

    public class IsValidEmail
    {
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("first.last@example.co.uk")]
        [InlineData("user+tag@example.com")]
        public void Should_ReturnTrue_ForValidEmail(string email)
        {
            Assert.True(email.IsValidEmail());
        }

        [Fact]
        public void Should_ReturnTrue_ForEmailWithLeadingAndTrailingWhitespace()
        {
            const string email = "  user@example.com  ";
            Assert.True(email.IsValidEmail());
        }

        [Fact]
        public void Should_ReturnFalse_ForEmptyString()
        {
            Assert.False("".IsValidEmail());
        }

        [Fact]
        public void Should_ReturnFalse_ForWhitespaceOnlyString()
        {
            Assert.False("   ".IsValidEmail());
        }

        [Fact]
        public void Should_ReturnFalse_ForEmailEndingInDot()
        {
            const string email = "user@example.com.";
            Assert.False(email.IsValidEmail());
        }

        [Fact]
        public void Should_ReturnFalse_ForEmailWithDisplayName()
        {
            const string email = "Some User <user@example.com>";
            Assert.False(email.IsValidEmail());
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("user@")]
        [InlineData("@example.com")]
        [InlineData("user@example@com")]
        public void Should_ReturnFalse_ForMalformedAddress(string email)
        {
            Assert.False(email.IsValidEmail());
        }
    }

    public class IsValidTag
    {
        [Theory]
        [InlineData("myUniqueHandle")]
        [InlineData("__myUniqueHandle")]
        [InlineData("_ab")]
        [InlineData("_e")]
        [InlineData("e_")]
        [InlineData("e-")]
        [InlineData("e.")]
        [InlineData("abc123")]
        public void Should_ReturnTrue_ForValidTag(string tag)
        {
            Assert.True(tag.IsValidTag());
        }

        [Theory]
        [InlineData("-ab")]
        [InlineData(".ab")]
        [InlineData("__")]
        [InlineData("")]
        [InlineData("user@example.com")]
        [InlineData("has space")]
        public void Should_ReturnFalse_ForInvalidTag(string tag)
        {
            Assert.False(tag.IsValidTag());
        }
    }
}