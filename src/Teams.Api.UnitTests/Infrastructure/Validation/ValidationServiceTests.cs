using FluentValidation;
using Teams.Api.Infrastructure.Validation;
using Teams.Core.Exceptions;

namespace Teams.Api.UnitTests.Infrastructure.Validation;

public static class ValidationServiceTests
{
    private sealed record ValidRequest(string Value);

    private sealed class ValidRequestValidator : AbstractValidator<ValidRequest>
    {
        public ValidRequestValidator()
            => RuleFor(request => request.Value).NotEmpty();
    }

    private sealed record UnregisteredRequest(string Value);

    private static ValidationService CreateSut()
        => new([new ValidRequestValidator()]);

    public class ValidateQueryAsync
    {
        [Fact]
        public async Task DoesNotThrow_WhenRequestIsValid()
        {
            var sut = CreateSut();
            var request = new ValidRequest("something");

            var exception = await Record.ExceptionAsync(
                () => sut.ValidateQueryAsync(request, TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ThrowsQueryValidationException_WhenRequestIsInvalid()
        {
            var sut = CreateSut();
            var request = new ValidRequest("");

            await Assert.ThrowsAsync<QueryValidationException>(
                () => sut.ValidateQueryAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ThrowsValidatorResolverException_WhenNoValidatorIsRegisteredForType()
        {
            var sut = CreateSut();
            var request = new UnregisteredRequest("something");

            await Assert.ThrowsAsync<ValidatorResolverException>(
                () => sut.ValidateQueryAsync(request, TestContext.Current.CancellationToken));
        }
    }

    public class ValidateCommandAsync
    {
        [Fact]
        public async Task DoesNotThrow_WhenRequestIsValid()
        {
            var sut = CreateSut();
            var request = new ValidRequest("something");

            var exception = await Record.ExceptionAsync(
                () => sut.ValidateCommandAsync(request, TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ThrowsCommandValidationException_WhenRequestIsInvalid()
        {
            var sut = CreateSut();
            var request = new ValidRequest("");

            await Assert.ThrowsAsync<CommandValidationException>(
                () => sut.ValidateCommandAsync(request, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ThrowsValidatorResolverException_WhenNoValidatorIsRegisteredForType()
        {
            var sut = CreateSut();
            var request = new UnregisteredRequest("something");

            await Assert.ThrowsAsync<ValidatorResolverException>(
                () => sut.ValidateCommandAsync(request, TestContext.Current.CancellationToken));
        }
    }
}