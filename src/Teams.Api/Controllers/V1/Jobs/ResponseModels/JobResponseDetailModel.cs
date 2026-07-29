namespace Teams.Api.Controllers.V1.Jobs.ResponseModels;

public record JobResponseDetailModel(
    string Id,
    string IdempotencyKey,
    string ConcurrencyToken,
    string Type,
    string Status,
    object? Parameters,
    DateTime DateCreated,
    DateTime DateLastModified,
    JobResponseErrorModel? Error = null)
    : JobResponseModel(Id, Status, IdempotencyKey, ConcurrencyToken, Error);