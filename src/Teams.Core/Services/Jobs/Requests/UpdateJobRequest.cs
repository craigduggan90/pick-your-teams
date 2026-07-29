namespace Teams.Core.Services.Jobs.Requests;

public record UpdateJobRequest(
    string Id,
    string ConcurrencyToken,
    string Status,
    string? ErrorCode,
    string? ErrorMessage);