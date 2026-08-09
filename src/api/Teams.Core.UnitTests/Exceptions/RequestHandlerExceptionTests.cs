using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class RequestHandlerExceptionTests
{
    public class ForCommandRequest
    {
        [Fact]
        public void ReturnsRequestHandlerException_WithMessageAndStatusCode422()
        {
            const string message = "Something went wrong.";

            var exception = RequestHandlerException.ForCommandRequest(message);

            Assert.Equal(message, exception.Message);
            Assert.Equal(422, exception.StatusCode);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ReturnsRequestHandlerException_WithInnerException_WhenProvided()
        {
            const string message = "Something went wrong.";
            var inner = new InvalidOperationException("inner failure");

            var exception = RequestHandlerException.ForCommandRequest(message, inner);

            Assert.Same(inner, exception.InnerException);
        }
    }

    public class ForQueryRequest
    {
        [Fact]
        public void ReturnsRequestHandlerException_WithMessageAndStatusCode400()
        {
            const string message = "Something went wrong.";

            var exception = RequestHandlerException.ForQueryRequest(message);

            Assert.Equal(message, exception.Message);
            Assert.Equal(400, exception.StatusCode);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ReturnsRequestHandlerException_WithInnerException_WhenProvided()
        {
            const string message = "Something went wrong.";
            var inner = new InvalidOperationException("inner failure");

            var exception = RequestHandlerException.ForQueryRequest(message, inner);

            Assert.Same(inner, exception.InnerException);
        }
    }
}