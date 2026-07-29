namespace Teams.Api.Controllers.V1.Jobs.ResponseModels;

public record JobResponseModel(
    string Id,
    string Status,
    string IdempotencyKey,
    string ConcurrencyToken,
    JobResponseErrorModel? Error = null);