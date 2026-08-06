using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class RequestHandlerRegistrationExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ShouldInitialiseObject_WithNoParameters()
        {
            var actual = new RequestHandlerRegistrationException();
            Assert.NotNull(actual);
        }

        [Fact]
        public void ShouldInitialiseObject_WithMessage()
        {
            const string message = "expected message";
            var actual = new RequestHandlerRegistrationException(message);
            Assert.Equal(message, actual.Message);
        }

        [Fact]
        public void ShouldInitialiseObject_WithMessage_AndInnerException()
        {
            const string message = "expected message";
            var innerException = new InvalidOperationException("this is an invalid operation.");
            var actual = new RequestHandlerRegistrationException(message, innerException);
            Assert.Equal(message, actual.Message);
            Assert.Same(innerException, actual.InnerException);
        }
    }

    public class ForRequest
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            var type = typeof(string);
            var expected = $"Unable to resolve handler for '{type.Name}' request.";
            var actual = RequestHandlerRegistrationException.ForRequest(type);
            Assert.Equal(expected, actual.Message);
        }
    }

    public class ForFailedInstantiation
    {
        [Fact]
        public void ShouldCreateException_WithExpectedMessage()
        {
            var type = typeof(string);
            var expected = $"Unable to instantiate handler for '{type.Name}' request.";
            var actual = RequestHandlerRegistrationException.ForFailedInstantiation(type);
            Assert.Equal(expected, actual.Message);
        }
    }
}