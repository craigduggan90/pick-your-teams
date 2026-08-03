using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class NotFoundExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static NotFoundExceptionHandler CreateSut() => new();

        [Fact]
        public async Task TryHandleAsync_ReturnsFalse_WhenExceptionNotNotFoundException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenNotFoundException_WithExtensions()
        {
            var exception = new NotFoundException("type", "identifier");

            const int expectedStatus = NotFoundExceptionHandler.StatusCode;
            var expectedContent = NotFoundExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(expectedStatus, context.Response.StatusCode);

            // This is a pain because of how Extensions are serialized.  Check the top-level properties first:
            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.NotNull(actualContent);
            Assert.Equal(expectedContent.Type, actualContent.Type);
            Assert.Equal(expectedContent.Title, actualContent.Title);
            Assert.Equal(expectedContent.Detail, actualContent.Detail);
            Assert.Equal(expectedContent.Status, actualContent.Status);

            // Then manually check the extensions because the collection gets weird:
            Assert.True(actualContent.Extensions.TryGetValue("resource", out var actualResourceObject));
            Assert.Equivalent(exception.ResourceType, actualResourceObject!.ToString());

            Assert.True(actualContent.Extensions.TryGetValue("identifier", out var actualResourceIdentifier));
            Assert.Equivalent(exception.ResourceIdentifier, actualResourceIdentifier!.ToString());
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenNotFoundException_WithoutExtensions()
        {
            var exception = new NotFoundException();

            const int expectedStatus = NotFoundExceptionHandler.StatusCode;
            var expectedContent = NotFoundExceptionHandler.GetProblemDetails(exception);

            using var responseStream = new MemoryStream();
            var context = new DefaultHttpContext { Response = { Body = responseStream } };

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(context, exception, CancellationToken.None);
            Assert.True(actual);
            Assert.Equal(expectedStatus, context.Response.StatusCode);

            var actualContent = await responseStream.RewindAndReadAsync<ProblemDetails>();
            Assert.Equivalent(expectedContent, actualContent);

            // We'll manually check the extensions:
            Assert.DoesNotContain("resource", actualContent!.Extensions.Keys);
            Assert.DoesNotContain("identifier", actualContent.Extensions.Keys);
        }
    }
}