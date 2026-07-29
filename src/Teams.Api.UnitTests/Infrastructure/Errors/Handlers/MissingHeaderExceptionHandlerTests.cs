using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Exceptions;
using Teams.Api.Infrastructure.Errors.Handlers;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class MissingHeaderExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static MissingHeaderExceptionHandler CreateSut() => new();

        [Fact]
        public async Task ReturnsFalse_WhenExceptionNotMissingHeaderException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ReturnsTrue_WhenMissingHeaderException()
        {
            var exception = new MissingHeaderException("Idempotency-Key");

            const int expectedStatus = MissingHeaderExceptionHandler.StatusCode;
            var expectedContent = MissingHeaderExceptionHandler.GetProblemDetails(exception);

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