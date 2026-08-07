using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Teams.Api.Attributes;
using Teams.Api.Exceptions;
using Teams.Core.Models;
using Teams.Core.Services;

namespace Teams.Api.UnitTests.Attributes;

public static class RequiresCurrentActorAttributeTests
{
    public class OnActionExecuting
    {
        private static ActionExecutingContext CreateContext(IActorAccessor actorAccessor)
        {
            var services = new ServiceCollection();
            services.AddSingleton(actorAccessor);
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            };

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
        public void ThrowsMissingHeaderException_WhenActorAccessorThrowsMissingHeaderException()
        {
            var actorAccessor = Substitute.For<IActorAccessor>();
            actorAccessor.Current.Returns(_ => throw new MissingHeaderException("Teams-User-Id"));
            var context = CreateContext(actorAccessor);
            var sut = new RequiresCurrentActorAttribute();

            Assert.Throws<MissingHeaderException>(() => sut.OnActionExecuting(context));
        }

        [Fact]
        public void DoesNotThrow_WhenActorAccessorResolvesSuccessfully()
        {
            var actorAccessor = Substitute.For<IActorAccessor>();
            actorAccessor.Current.Returns(new Actor("user-001", "tag-001", "display-name"));
            var context = CreateContext(actorAccessor);
            var sut = new RequiresCurrentActorAttribute();

            var exception = Record.Exception(() => sut.OnActionExecuting(context));

            Assert.Null(exception);
        }

        [Fact]
        public void AccessesCurrentExactlyOnce()
        {
            var actorAccessor = Substitute.For<IActorAccessor>();
            actorAccessor.Current.Returns(new Actor("user-001", "tag-001", "display-name"));
            var context = CreateContext(actorAccessor);
            var sut = new RequiresCurrentActorAttribute();

            sut.OnActionExecuting(context);

            _ = actorAccessor.Received(1).Current;
        }
    }
}