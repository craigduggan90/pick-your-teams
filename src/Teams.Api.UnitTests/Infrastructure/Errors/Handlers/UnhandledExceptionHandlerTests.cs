using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Testing;
using Teams.Api.Infrastructure.Errors.Handlers;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class UnhandledExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private readonly FakeLogger<UnhandledExceptionHandler> _logger = new();

        private UnhandledExceptionHandler CreateSut() => new(_logger);

        [Fact]
        public async Task ShouldReturnTrue_WhenException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var expectedStatus = UnhandledExceptionHandler.StatusCode;
            var expectedContent = UnhandledExceptionHandler.GetProblemDetails();

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(expectedStatus, context.Response.StatusCode);

            // Check the content
            Assert.Equivalent(expectedContent, await responseStream.RewindAndReadAsync<ProblemDetails>());
        }
    }
}