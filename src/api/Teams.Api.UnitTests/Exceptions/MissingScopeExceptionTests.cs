using Teams.Api.Exceptions;

namespace Teams.Api.UnitTests.Exceptions;

public static class MissingScopeExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void UsesSingularMessage_WhenOneScopeProvided()
        {
            var exception = new MissingScopeException("jobs:write");

            Assert.Equal("Required scope was not present: 'jobs:write'.", exception.Message);
        }

        [Fact]
        public void UsesPluralMessage_WhenMultipleScopesProvided()
        {
            var exception = new MissingScopeException("jobs:write", "jobs:read");

            Assert.Equal("Required scopes were not present: 'jobs:write', 'jobs:read'.", exception.Message);
        }
    }
}