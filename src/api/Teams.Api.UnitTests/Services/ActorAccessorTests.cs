using Microsoft.AspNetCore.Http;
using NSubstitute;
using Teams.Api.Exceptions;
using Teams.Api.Services;

namespace Teams.Api.UnitTests.Services;

public static class ActorAccessorTests
{
    public class Current
    {
        private static IHttpContextAccessor CreateContextAccessor(HttpContext? httpContext)
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns(httpContext);
            return accessor;
        }

        private static HttpContext CreateHttpContext(
            string? userId = "user-001", string? userTag = "tag-001", string? userName = "display-name")
        {
            var context = new DefaultHttpContext();
            if (userId is not null)
                context.Request.Headers["Teams-User-Id"] = userId;
            if (userTag is not null)
                context.Request.Headers["Teams-User-Tag"] = userTag;
            if (userName is not null)
                context.Request.Headers["Teams-User-Name"] = userName;
            return context;
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenNoHttpContextAvailable()
        {
            var sut = new ActorAccessor(CreateContextAccessor(null));

            Assert.Throws<InvalidOperationException>(() => sut.Current);
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenUserIdHeaderMissing()
        {
            var httpContext = CreateHttpContext(userId: null);
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var exception = Assert.Throws<MissingHeaderException>(() => sut.Current);
            Assert.Contains("Teams-User-Id", exception.Message);
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenUserTagHeaderMissing()
        {
            var httpContext = CreateHttpContext(userTag: null);
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var exception = Assert.Throws<MissingHeaderException>(() => sut.Current);
            Assert.Contains("Teams-User-Tag", exception.Message);
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenUserNameHeaderMissing()
        {
            var httpContext = CreateHttpContext(userName: null);
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var exception = Assert.Throws<MissingHeaderException>(() => sut.Current);
            Assert.Contains("Teams-User-Name", exception.Message);
        }

        [Fact]
        public void ThrowsMissingHeaderException_WhenHeaderValueIsWhitespace()
        {
            var httpContext = CreateHttpContext(userId: "   ");
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var exception = Assert.Throws<MissingHeaderException>(() => sut.Current);
            Assert.Contains("Teams-User-Id", exception.Message);
        }

        [Fact]
        public void ReturnsActor_WithHeaderValues_WhenAllHeadersPresent()
        {
            var httpContext = CreateHttpContext();
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var actor = sut.Current;

            Assert.Equal("user-001", actor.Id);
            Assert.Equal("tag-001", actor.Tag);
            Assert.Equal("display-name", actor.DisplayName);
        }

        [Fact]
        public void ReturnsSameActor_OnRepeatedAccess()
        {
            var httpContext = CreateHttpContext();
            var sut = new ActorAccessor(CreateContextAccessor(httpContext));

            var first = sut.Current;
            var second = sut.Current;

            Assert.Equal(first, second);
        }
    }
}