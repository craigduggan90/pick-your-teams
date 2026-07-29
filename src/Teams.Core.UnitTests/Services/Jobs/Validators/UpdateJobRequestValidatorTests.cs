using FluentValidation.TestHelper;
using Teams.Core.Services.Jobs.Requests;
using Teams.Core.Services.Jobs.Validators;

namespace Teams.Core.UnitTests.Services.Jobs.Validators;

public static class UpdateJobRequestValidatorTests
{
    public abstract class UpdateJobRequestValidatorTestsBase
    {
        protected readonly UpdateJobRequestValidator Validator = new();

        protected static UpdateJobRequest CreateRequest(
            string id = "job-id-001",
            string concurrencyToken = "",
            string status = "InProgress",
            string? errorCode = null,
            string? errorMessage = null)
            => new(id, concurrencyToken, status, errorCode, errorMessage);
    }

    public class Id : UpdateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenEmpty()
        {
            var request = CreateRequest(id: "");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Id);
        }

        [Fact]
        public void HasNoError_WhenValid()
        {
            var request = CreateRequest(id: "job-id-001");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Id);
        }
    }

    public class Status : UpdateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenNotAValidEnumName()
        {
            var request = CreateRequest(status: "NotARealStatus");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasError_WhenEmpty()
        {
            var request = CreateRequest(status: "");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumName()
        {
            var request = CreateRequest(status: "Complete");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumNameWithDifferentCasing()
        {
            var request = CreateRequest(status: "complete");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Status);
        }
    }

    public class ErrorCode : UpdateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenExceedsMaximumLength()
        {
            var request = CreateRequest(status: "Failed", errorCode: new string('a', 101), errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasNoError_WhenAtMaximumLength()
        {
            var request = CreateRequest(status: "Failed", errorCode: new string('a', 100), errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasError_WhenStatusIsFailedAndErrorCodeIsEmpty()
        {
            var request = CreateRequest(status: "Failed", errorCode: "", errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasError_WhenStatusIsFailedAndErrorCodeIsNull()
        {
            var request = CreateRequest(status: "Failed", errorCode: null, errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasNoError_WhenStatusIsFailedAndErrorCodeIsProvided()
        {
            var request = CreateRequest(status: "Failed", errorCode: "SOME_CODE", errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasError_WhenStatusIsNotFailedAndErrorCodeIsProvided()
        {
            var request = CreateRequest(status: "InProgress", errorCode: "SOME_CODE");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorCode);
        }

        [Fact]
        public void HasNoError_WhenStatusIsNotFailedAndErrorCodeIsNull()
        {
            var request = CreateRequest(status: "InProgress", errorCode: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorCode);
        }
    }

    public class ErrorMessage : UpdateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenExceedsMaximumLength()
        {
            var request = CreateRequest(status: "Failed", errorCode: "SOME_CODE", errorMessage: new string('a', 256));

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorMessage);
        }

        [Fact]
        public void HasNoError_WhenAtMaximumLength()
        {
            var request = CreateRequest(status: "Failed", errorCode: "SOME_CODE", errorMessage: new string('a', 255));

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorMessage);
        }

        [Fact]
        public void HasError_WhenStatusIsFailedAndErrorMessageIsEmpty()
        {
            var request = CreateRequest(status: "Failed", errorCode: "SOME_CODE", errorMessage: "");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorMessage);
        }

        [Fact]
        public void HasNoError_WhenStatusIsFailedAndErrorMessageIsProvided()
        {
            var request = CreateRequest(status: "Failed", errorCode: "SOME_CODE", errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorMessage);
        }

        [Fact]
        public void HasError_WhenStatusIsNotFailedAndErrorMessageIsProvided()
        {
            var request = CreateRequest(status: "InProgress", errorMessage: "some message");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.ErrorMessage);
        }

        [Fact]
        public void HasNoError_WhenStatusIsNotFailedAndErrorMessageIsNull()
        {
            var request = CreateRequest(status: "InProgress", errorMessage: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.ErrorMessage);
        }
    }
}