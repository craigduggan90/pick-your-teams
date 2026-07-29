using Teams.Api.Controllers.V1.Jobs;
using Teams.Api.Controllers.V1.Jobs.RequestModels;
using Teams.Common.Extensions;
using Teams.Core.Services.Jobs.Responses;
using System.Text.Json;

namespace Teams.Api.UnitTests.Controllers.V1.Jobs;

public static class JobsMapperTests
{
    private static JobModel CreateJobModel(
        string id = "job-id-001",
        string idempotencyKey = "idempotency-key-001",
        string concurrencyToken = "concurrency-token-001",
        string type = "ArchiveProjectJob",
        string status = "Pending",
        string? parameters = null,
        string? errorCode = null,
        string? errorMessage = null,
        DateTime? dateCreated = null,
        DateTime? dateModified = null)
        => new(
            id,
            idempotencyKey,
            concurrencyToken,
            type,
            status,
            parameters,
            errorCode,
            errorMessage,
            dateCreated ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            dateModified ?? new DateTime(2026, 1, 1, 12, 5, 0, DateTimeKind.Utc));

    public class ToJobResponseModel
    {
        [Fact]
        public void MapsIdStatusIdempotencyKeyAndConcurrencyToken_WhenCalled()
        {
            var model = CreateJobModel();

            var result = model.ToJobResponseModel();

            Assert.Equal(model.Id, result.Id);
            Assert.Equal(model.Status, result.Status);
            Assert.Equal(model.IdempotencyKey, result.IdempotencyKey);
            Assert.Equal(model.ConcurrencyToken, result.ConcurrencyToken);
        }

        [Fact]
        public void SetsErrorToNull_WhenModelHasNoErrorCode()
        {
            var model = CreateJobModel(errorCode: null);

            var result = model.ToJobResponseModel();

            Assert.Null(result.Error);
        }

        [Fact]
        public void SetsError_WhenModelHasErrorCode()
        {
            var model = CreateJobModel(errorCode: "SOME_CODE", errorMessage: "some message");

            var result = model.ToJobResponseModel();

            Assert.NotNull(result.Error);
            Assert.Equal("SOME_CODE", result.Error.Code);
            Assert.Equal("some message", result.Error.Message);
        }

        [Fact]
        public void SetsErrorMessageToDefault_WhenErrorCodeIsSetButErrorMessageIsNull()
        {
            var model = CreateJobModel(errorCode: "SOME_CODE", errorMessage: null);

            var result = model.ToJobResponseModel();

            Assert.NotNull(result.Error);
            Assert.Equal("An unexpected error occurred.", result.Error.Message);
        }
    }

    public class ToJobResponseDetailModel
    {
        [Fact]
        public void MapsIdentityTypeStatusAndTimestamps_WhenCalled()
        {
            var model = CreateJobModel();

            var result = model.ToJobResponseDetailModel();

            Assert.Equal(model.Id, result.Id);
            Assert.Equal(model.IdempotencyKey, result.IdempotencyKey);
            Assert.Equal(model.ConcurrencyToken, result.ConcurrencyToken);
            Assert.Equal(model.Type, result.Type);
            Assert.Equal(model.Status, result.Status);
            Assert.Equal(model.DateCreated, result.DateCreated);
            Assert.Equal(model.DateModified, result.DateLastModified);
        }

        [Fact]
        public void SetsParametersToNull_WhenModelParametersIsNull()
        {
            var model = CreateJobModel(parameters: null);

            var result = model.ToJobResponseDetailModel();

            Assert.Null(result.Parameters);
        }

        [Fact]
        public void SetsParameters_WhenModelParametersIsProvided()
        {
            var model = CreateJobModel(parameters: """{"foo":"bar"}""");

            var result = model.ToJobResponseDetailModel();

            var parameters = Assert.IsType<JsonElement>(result.Parameters);
            Assert.Equal("bar", parameters.GetProperty("foo").GetString());
        }

        [Fact]
        public void SetsErrorToNull_WhenModelHasNoErrorCode()
        {
            var model = CreateJobModel(errorCode: null);

            var result = model.ToJobResponseDetailModel();

            Assert.Null(result.Error);
        }

        [Fact]
        public void SetsError_WhenModelHasErrorCode()
        {
            var model = CreateJobModel(errorCode: "SOME_CODE", errorMessage: "some message");

            var result = model.ToJobResponseDetailModel();

            Assert.NotNull(result.Error);
            Assert.Equal("SOME_CODE", result.Error.Code);
            Assert.Equal("some message", result.Error.Message);
        }
    }

    public class ToCreateJobRequest
    {
        [Fact]
        public void SetsIdempotencyKeyToEmptyString_WhenIdempotencyKeyIsNull()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", null);

            var result = model.ToCreateJobRequest(null);

            Assert.Equal(string.Empty, result.IdempotencyKey);
        }

        [Fact]
        public void SetsIdempotencyKey_WhenProvided()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", null);

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Equal("idempotency-key-001", result.IdempotencyKey);
        }

        [Fact]
        public void SetsType_WhenCalled()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", null);

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Equal("ArchiveProjectJob", result.Type);
        }

        [Fact]
        public void SetsParametersToNull_WhenParametersIsNull()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", null);

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Null(result.Parameters);
        }

        [Fact]
        public void SetsParametersToNull_WhenParametersValueKindIsJsonNull()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", "null".Deserialize<JsonElement>());

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Null(result.Parameters);
        }

        [Fact]
        public void SetsParametersToNull_WhenParametersValueKindIsUndefined()
        {
            var model = new CreateJobRequestModel("ArchiveProjectJob", default(JsonElement));

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Null(result.Parameters);
        }

        [Fact]
        public void SetsParametersToRawText_WhenParametersIsProvided()
        {
            const string json = """{"foo":"bar"}""";
            var model = new CreateJobRequestModel("ArchiveProjectJob", json.Deserialize<JsonElement>());

            var result = model.ToCreateJobRequest("idempotency-key-001");

            Assert.Equal(json, result.Parameters);
        }
    }

    public class ToUpdateJobRequest
    {
        [Fact]
        public void SetsId_WhenCalled()
        {
            var model = new UpdateJobRequestModel("InProgress", null, null);

            var result = model.ToUpdateJobRequest("job-id-001", "concurrency-token-001");

            Assert.Equal("job-id-001", result.Id);
        }

        [Fact]
        public void SetsConcurrencyTokenToEmptyString_WhenConcurrencyTokenIsNull()
        {
            var model = new UpdateJobRequestModel("InProgress", null, null);

            var result = model.ToUpdateJobRequest("job-id-001", null);

            Assert.Equal(string.Empty, result.ConcurrencyToken);
        }

        [Fact]
        public void SetsConcurrencyToken_WhenProvided()
        {
            var model = new UpdateJobRequestModel("InProgress", null, null);

            var result = model.ToUpdateJobRequest("job-id-001", "concurrency-token-001");

            Assert.Equal("concurrency-token-001", result.ConcurrencyToken);
        }

        [Fact]
        public void SetsStatusErrorCodeAndErrorMessage_WhenCalled()
        {
            var model = new UpdateJobRequestModel("Failed", "SOME_CODE", "some message");

            var result = model.ToUpdateJobRequest("job-id-001", "concurrency-token-001");

            Assert.Equal("Failed", result.Status);
            Assert.Equal("SOME_CODE", result.ErrorCode);
            Assert.Equal("some message", result.ErrorMessage);
        }
    }

    public class ToGetJobsRequestRequest
    {
        [Fact]
        public void MapsAllProperties_WhenCalled()
        {
            var model = new GetJobsRequestModel(
                Type: "ArchiveProjectJob",
                Status: "Failed",
                ErrorCode: "SOME_CODE",
                CreatedFrom: new DateTime(2026, 1, 1),
                CreatedTo: new DateTime(2026, 1, 31),
                ModifiedFrom: new DateTime(2026, 1, 5),
                ModifiedTo: new DateTime(2026, 1, 25),
                Cursor: "some-cursor",
                PageSize: 10);

            var result = model.ToGetJobsRequestRequest();

            Assert.Equal(model.Type, result.Type);
            Assert.Equal(model.Status, result.Status);
            Assert.Equal(model.ErrorCode, result.ErrorCode);
            Assert.Equal(model.CreatedFrom, result.CreatedFrom);
            Assert.Equal(model.CreatedTo, result.CreatedTo);
            Assert.Equal(model.ModifiedFrom, result.ModifiedFrom);
            Assert.Equal(model.ModifiedTo, result.ModifiedTo);
            Assert.Equal(model.Cursor, result.Cursor);
            Assert.Equal(model.PageSize, result.PageSize);
        }
    }
}