using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class RequestHandlerExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static RequestHandlerExceptionHandler CreateSut() => new();

        [Fact]
        public async Task ShouldReturnFalse_WhenExceptionNotRequestHandlerException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenCommandRequestException()
        {
            var exception = RequestHandlerException.ForCommandRequest("Something went wrong.");

            var expectedContent = RequestHandlerExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(422, context.Response.StatusCode);

            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.Equivalent(expectedContent, actualContent);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenQueryRequestException()
        {
            var exception = RequestHandlerException.ForQueryRequest("Something went wrong.");

            var expectedContent = RequestHandlerExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(400, context.Response.StatusCode);

            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.Equivalent(expectedContent, actualContent);
        }

        [Fact]
        public void GetProblemDetails_MapsExceptionToProblemDetails()
        {
            var exception = RequestHandlerException.ForCommandRequest("Something went wrong.");

            var problemDetails = RequestHandlerExceptionHandler.GetProblemDetails(exception);

            Assert.Equal("Service Error", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
            Assert.Equal(exception.StatusCode, problemDetails.Status);
            Assert.EndsWith("/service", problemDetails.Type);
        }
    }
}