using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Teams.Api.Attributes;
using Teams.Api.Exceptions;
using Teams.Common;

namespace Teams.Api.UnitTests.Attributes;

public static class RequiresScopeAttributeTests
{
    public class OnActionExecuting
    {
        private static ActionExecutingContext CreateContext(string? scopeHeaderValue = null)
        {
            var httpContext = new DefaultHttpContext();

            if (scopeHeaderValue is not null)
                httpContext.Request.Headers[Constants.ScopeHeaderKey] = scopeHeaderValue;

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?>(),
                controller: new object());
        }

        [Fact]
        public void ThrowsMissingScopeException_WhenScopeHeaderIsNotPresent()
        {
            var context = CreateContext();
            var sut = new RequiresScopeAttribute("jobs:write");

            Assert.Throws<MissingScopeException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void ThrowsMissingScopeException_WhenScopeHeaderIsEmpty()
        {
            var context = CreateContext("");
            var sut = new RequiresScopeAttribute("jobs:write");

            Assert.Throws<MissingScopeException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void ThrowsMissingScopeException_WhenScopeHeaderDoesNotContainRequiredScope()
        {
            var context = CreateContext("jobs:read");
            var sut = new RequiresScopeAttribute("jobs:write");

            Assert.Throws<MissingScopeException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void DoesNotThrow_WhenScopeHeaderContainsOnlyRequiredScope()
        {
            var context = CreateContext("jobs:write");
            var sut = new RequiresScopeAttribute("jobs:write");

            var exception = Record.Exception(() => sut.OnActionExecuting(context));

            Assert.Null(exception);
        }

        [Fact]
        public void DoesNotThrow_WhenScopeHeaderContainsRequiredScopeAmongOthers()
        {
            var context = CreateContext("jobs:read, jobs:write, jobs:delete");
            var sut = new RequiresScopeAttribute("jobs:write");

            var exception = Record.Exception(() => sut.OnActionExecuting(context));

            Assert.Null(exception);
        }

        [Fact]
        public void DoesNotThrow_WhenScopeHeaderMatchesRequiredScopeCaseInsensitively()
        {
            var context = CreateContext("JOBS:WRITE");
            var sut = new RequiresScopeAttribute("jobs:write");

            var exception = Record.Exception(() => sut.OnActionExecuting(context));

            Assert.Null(exception);
        }
    }
}