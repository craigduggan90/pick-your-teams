using FluentValidation.TestHelper;
using Teams.Core.Services.Jobs.Requests;
using Teams.Core.Services.Jobs.Validators;

namespace Teams.Core.UnitTests.Services.Jobs.Validators;

public static class CreateJobRequestValidatorTests
{
    public abstract class CreateJobRequestValidatorTestsBase
    {
        protected readonly CreateJobRequestValidator Validator = new();

        protected static CreateJobRequest CreateRequest(
            string idempotencyKey = "idempotency-key-001",
            string type = "ArchiveProjectJob",
            string? parameters = null)
            => new(idempotencyKey, type, parameters);
    }

    public class IdempotencyKey : CreateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenEmpty()
        {
            var request = CreateRequest(idempotencyKey: "");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.IdempotencyKey);
        }

        [Fact]
        public void HasError_WhenExceedsMaximumLength()
        {
            var request = CreateRequest(idempotencyKey: new string('a', 101));

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.IdempotencyKey);
        }

        [Fact]
        public void HasNoError_WhenAtMaximumLength()
        {
            var request = CreateRequest(idempotencyKey: new string('a', 100));

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.IdempotencyKey);
        }

        [Fact]
        public void HasNoError_WhenValid()
        {
            var request = CreateRequest(idempotencyKey: "a-valid-key");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.IdempotencyKey);
        }
    }

    public class Type : CreateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenNotAValidEnumName()
        {
            var request = CreateRequest(type: "NotARealJobType");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Type);
        }

        [Fact]
        public void HasError_WhenEmpty()
        {
            var request = CreateRequest(type: "");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Type);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumName()
        {
            var request = CreateRequest(type: "ArchiveProjectJob");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Type);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumNameWithDifferentCasing()
        {
            var request = CreateRequest(type: "archiveprojectjob");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Type);
        }
    }

    public class Parameters : CreateJobRequestValidatorTestsBase
    {
        [Fact]
        public void HasError_WhenExceedsMaximumLength()
        {
            var request = CreateRequest(parameters: new string('a', 1001));

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Parameters);
        }

        [Fact]
        public void HasNoError_WhenAtMaximumLength()
        {
            var request = CreateRequest(parameters: new string('a', 1000));

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Parameters);
        }

        [Fact]
        public void HasNoError_WhenNull()
        {
            var request = CreateRequest(parameters: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Parameters);
        }
    }
}