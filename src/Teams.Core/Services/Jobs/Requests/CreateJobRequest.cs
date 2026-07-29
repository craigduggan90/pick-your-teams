namespace Teams.Core.Services.Jobs.Requests;

public record CreateJobRequest(
    string IdempotencyKey,
    string Type,
    string? Parameters);