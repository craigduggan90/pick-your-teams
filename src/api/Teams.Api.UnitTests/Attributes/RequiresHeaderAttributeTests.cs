using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Teams.Api.Attributes;
using Teams.Api.Exceptions;

namespace Teams.Api.UnitTests.Attributes;

public static class RequiresHeaderAttributeTests
{
    public class OnActionExecuting
    {
        private static ActionExecutingContext CreateContext(string? headerName = null, string? headerValue = null)
        {
            var httpContext = new DefaultHttpContext();

            if (headerName is not null && headerValue is not null)
                httpContext.Request.Headers[headerName] = headerValue;

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
        public void ThrowsMissingHeaderException_WhenHeaderIsNotPresent()
        {
            var context = CreateContext();
            var sut = new RequiresHeaderAttribute("Idempotency-Key");

            Assert.Throws<MissingHeaderException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenHeaderIsEmpty()
        {
            var context = CreateContext("Idempotency-Key", "");
            var sut = new RequiresHeaderAttribute("Idempotency-Key");

            Assert.Throws<MissingHeaderException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenHeaderIsWhitespace()
        {
            var context = CreateContext("Idempotency-Key", "   ");
            var sut = new RequiresHeaderAttribute("Idempotency-Key");

            Assert.Throws<MissingHeaderException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void DoesNotThrow_WhenHeaderIsPresentAndHasValue()
        {
            var context = CreateContext("Idempotency-Key", "some-value");
            var sut = new RequiresHeaderAttribute("Idempotency-Key");

            var exception = Record.Exception(() => sut.OnActionExecuting(context));

            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenADifferentHeaderIsPresentButNotTheRequiredOne()
        {
            var context = CreateContext("Some-Other-Header", "some-value");
            var sut = new RequiresHeaderAttribute("Idempotency-Key");

            Assert.Throws<MissingHeaderException>(() => sut.OnActionExecuting(context));
        }
    }
}