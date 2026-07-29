namespace Teams.Api.Controllers.V1.Jobs.RequestModels;

public record UpdateJobRequestModel(
    string Status,
    string? ErrorCode,
    string? ErrorMessage);