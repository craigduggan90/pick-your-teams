using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class ValidatorResolverExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void SetsMessage_WhenConstructed()
        {
            var exception = new ValidatorResolverException("No validator found for 'SomeRequest'.");

            Assert.Equal("No validator found for 'SomeRequest'.", exception.Message);
        }

        [Fact]
        public void SetsInnerExceptionToNull_WhenNotProvided()
        {
            var exception = new ValidatorResolverException("No validator found for 'SomeRequest'.");

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void SetsInnerException_WhenProvided()
        {
            var inner = new InvalidOperationException("Underlying failure.");

            var exception = new ValidatorResolverException("No validator found for 'SomeRequest'.", inner);

            Assert.Same(inner, exception.InnerException);
        }
    }
}