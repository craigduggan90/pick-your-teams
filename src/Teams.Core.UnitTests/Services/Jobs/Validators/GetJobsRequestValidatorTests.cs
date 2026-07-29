using FluentValidation.TestHelper;
using Teams.Common.Pagination;
using Teams.Core.Services.Jobs.Requests;
using Teams.Core.Services.Jobs.Validators;

namespace Teams.Core.UnitTests.Services.Jobs.Validators;

public static class GetJobsRequestValidatorTests
{
    public abstract class GetJobsRequestValidatorTestsBase
    {
        protected readonly GetJobsRequestValidator Validator = new();

        protected static string ValidCursor()
        {
            ((long?)12345L).TryEncodeCursor(out var cursor);
            return cursor!;
        }

        protected static GetJobsRequest CreateRequest(
            string? type = null,
            string? status = null,
            string? cursor = null,
            int? pageSize = null)
            => new(Type: type, Status: status, Cursor: cursor, PageSize: pageSize);
    }

    public class Cursor : GetJobsRequestValidatorTestsBase
    {
        [Fact]
        public void HasNoError_WhenNull()
        {
            var request = CreateRequest(cursor: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Cursor);
        }

        [Fact]
        public void HasNoError_WhenValid()
        {
            var request = CreateRequest(cursor: ValidCursor());

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Cursor);
        }

        [Fact]
        public void HasError_WhenNotValidBase64()
        {
            var request = CreateRequest(cursor: "not-valid-base64!!!");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Cursor);
        }

        [Fact]
        public void HasError_WhenDecodedValueIsNotANumber()
        {
            var invalidCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-a-number"));
            var request = CreateRequest(cursor: invalidCursor);

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Cursor);
        }
    }

    public class Type : GetJobsRequestValidatorTestsBase
    {
        [Fact]
        public void HasNoError_WhenNull()
        {
            var request = CreateRequest(type: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Type);
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

        [Fact]
        public void HasError_WhenNotAValidEnumName()
        {
            var request = CreateRequest(type: "NotARealJobType");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Type);
        }
    }

    public class Status : GetJobsRequestValidatorTestsBase
    {
        [Fact]
        public void HasNoError_WhenNull()
        {
            var request = CreateRequest(status: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumName()
        {
            var request = CreateRequest(status: "Failed");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasNoError_WhenAValidEnumNameWithDifferentCasing()
        {
            var request = CreateRequest(status: "failed");

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.Status);
        }

        [Fact]
        public void HasError_WhenNotAValidEnumName()
        {
            var request = CreateRequest(status: "NotARealStatus");

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.Status);
        }
    }

    public class PageSize : GetJobsRequestValidatorTestsBase
    {
        [Fact]
        public void HasNoError_WhenNull()
        {
            var request = CreateRequest(pageSize: null);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.PageSize);
        }

        [Fact]
        public void HasError_WhenZero()
        {
            var request = CreateRequest(pageSize: 0);

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.PageSize);
        }

        [Fact]
        public void HasError_WhenNegative()
        {
            var request = CreateRequest(pageSize: -1);

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.PageSize);
        }

        [Fact]
        public void HasNoError_WhenOne()
        {
            var request = CreateRequest(pageSize: 1);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.PageSize);
        }

        [Fact]
        public void HasError_WhenGreaterThanMaxPageSize()
        {
            var request = CreateRequest(pageSize: 101);

            var result = Validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(r => r.PageSize);
        }

        [Fact]
        public void HasNoError_WhenEqualToMaxPageSize()
        {
            var request = CreateRequest(pageSize: 100);

            var result = Validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(r => r.PageSize);
        }
    }
}