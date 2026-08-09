using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Exceptions;
using Teams.Api.Infrastructure.Errors.Handlers;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class MissingScopeExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static MissingScopeExceptionHandler CreateSut() => new();

        [Fact]
        public async Task ReturnsFalse_WhenExceptionNotMissingScopeException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ReturnsTrue_WhenMissingScopeException()
        {
            var exception = new MissingScopeException("jobs:write");

            const int expectedStatus = MissingScopeExceptionHandler.StatusCode;
            var expectedContent = MissingScopeExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(expectedStatus, context.Response.StatusCode);

            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.Equivalent(expectedContent, actualContent);
        }
    }
}