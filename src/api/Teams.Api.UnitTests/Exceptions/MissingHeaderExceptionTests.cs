using Teams.Api.Exceptions;

namespace Teams.Api.UnitTests.Exceptions;

public static class MissingHeaderExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void IncludesHeaderName_InMessage()
        {
            const string headerName = "If-Match";

            var exception = new MissingHeaderException(headerName);

            Assert.Contains(headerName, exception.Message);
        }
    }
}