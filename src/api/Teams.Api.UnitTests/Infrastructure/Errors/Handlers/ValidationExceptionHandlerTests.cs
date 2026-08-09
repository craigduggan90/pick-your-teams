using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Teams.Api.Infrastructure.Errors.Handlers;
using Teams.Common.Extensions;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Errors.Handlers;

public static class ValidationExceptionHandlerTests
{
    public class TryHandleAsync
    {
        private static ValidationExceptionHandler CreateSut() => new();

        [Fact]
        public async Task TryHandleAsync_ReturnsFalse_WhenExceptionNotValidationException()
        {
            var exception = new InvalidOperationException("that was an invalid operation!");

            var sut = CreateSut();
            var actual = await sut.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);
            Assert.False(actual);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenCommandValidationException()
        {
            var exception = new CommandValidationException([
                new ValidationFailure("one", "one"),
                new ValidationFailure("one", "two"),
                new ValidationFailure("one", "three"),
                new ValidationFailure("two", "one"),
                new ValidationFailure("two", "two"),
                new ValidationFailure("three", "one")
            ]);

            const int expectedStatus = 422;
            var expectedContent = ValidationExceptionHandler.GetProblemDetails(expectedStatus, exception);

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
            Dictionary<string, string[]> expectedErrors = new()
            {
                { "one", ["one", "two", "three"] },
                { "two", ["one", "two"] },
                { "three", ["one"] }
            };
            Assert.True(actualContent.Extensions.TryGetValue("errors", out var actualErrorsObject));
            var actualErrors = actualErrorsObject.Serialize().Deserialize<Dictionary<string, string[]>>();
            Assert.Equivalent(expectedErrors, actualErrors, true);
        }

        [Fact]
        public async Task ShouldReturnTrue_WhenQueryValidationException()
        {
            var exception = new QueryValidationException([
                new ValidationFailure("one", "one"),
                new ValidationFailure("one", "two"),
                new ValidationFailure("one", "three"),
                new ValidationFailure("two", "one"),
                new ValidationFailure("two", "two"),
                new ValidationFailure("three", "one")
            ]);

            const int expectedStatus = 400;
            var expectedContent = ValidationExceptionHandler.GetProblemDetails(expectedStatus, exception);

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
            Dictionary<string, string[]> expectedErrors = new()
            {
                { "one", ["one", "two", "three"] },
                { "two", ["one", "two"] },
                { "three", ["one"] }
            };
            Assert.True(actualContent.Extensions.TryGetValue("errors", out var actualErrorsObject));
            var actualErrors = actualErrorsObject.Serialize().Deserialize<Dictionary<string, string[]>>();
            Assert.Equivalent(expectedErrors, actualErrors, true);
        }
    }
}