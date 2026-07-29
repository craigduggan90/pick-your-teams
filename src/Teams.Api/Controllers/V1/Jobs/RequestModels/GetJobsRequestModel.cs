namespace Teams.Api.Controllers.V1.Jobs.RequestModels;

public record GetJobsRequestModel(
    string? Type = null,
    string? Status = null,
    string? ErrorCode = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    string? Cursor = null,
    int? PageSize = null);