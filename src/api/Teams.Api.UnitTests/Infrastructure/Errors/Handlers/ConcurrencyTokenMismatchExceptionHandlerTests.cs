using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class ConcurrencyTokenMismatchExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static ConcurrencyTokenMismatchExceptionHandler CreateSut() => new();

        [Fact]
        public async Task ReturnsFalse_WhenExceptionNotConcurrencyTokenMismatchException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ReturnsTrue_WhenConcurrencyTokenMismatchException()
        {
            var exception = new ConcurrencyTokenMismatchException();

            const int expectedStatus = ConcurrencyTokenMismatchExceptionHandler.StatusCode;
            var expectedContent = ConcurrencyTokenMismatchExceptionHandler.GetProblemDetails(exception);

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