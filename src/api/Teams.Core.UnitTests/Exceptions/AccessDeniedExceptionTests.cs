using Teams.Core.Exceptions;

namespace Teams.Core.UnitTests.Exceptions;

public static class AccessDeniedExceptionTests
{
    public class ForOrganiserOnly
    {
        [Fact]
        public void ReturnsAccessDeniedException_WithOrganiserMessage()
        {
            var exception = AccessDeniedException.ForOrganiserOnly();

            Assert.Equal("Action only available to game organiser.", exception.Message);
        }
    }

    public class ForSelfOnly
    {
        [Fact]
        public void ReturnsAccessDeniedException_WithSubjectUserMessage()
        {
            var exception = AccessDeniedException.ForSelfOnly();

            Assert.Equal("Action only available to subject user.", exception.Message);
        }
    }

    public class ForOrganiserOrSelfOnly
    {
        [Fact]
        public void ReturnsAccessDeniedException_WithOrganiserOrSubjectUserMessage()
        {
            var exception = AccessDeniedException.ForOrganiserOrSelfOnly();

            Assert.Equal("Action only available to game organiser or subject user.", exception.Message);
        }
    }
}