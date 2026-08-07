using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class AccessDeniedExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static AccessDeniedExceptionHandler CreateSut() => new();

        [Fact]
        public async Task ShouldReturnFalse_WhenExceptionNotAccessDeniedException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenAccessDeniedException()
        {
            var exception = AccessDeniedException.ForOrganiserOnly();

            const int expectedStatus = AccessDeniedExceptionHandler.StatusCode;
            var expectedContent = AccessDeniedExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(expectedStatus, context.Response.StatusCode);

            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.Equivalent(expectedContent, actualContent);
        }

        [Fact]
        public void GetProblemDetails_MapsExceptionToProblemDetails()
        {
            var exception = AccessDeniedException.ForSelfOnly();

            var problemDetails = AccessDeniedExceptionHandler.GetProblemDetails(exception);

            Assert.Equal("Forbidden", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
            Assert.Equal(AccessDeniedExceptionHandler.StatusCode, problemDetails.Status);
            Assert.EndsWith("/forbidden", problemDetails.Type);
        }
    }
}