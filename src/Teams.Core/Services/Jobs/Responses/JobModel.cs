using Teams.Domain.Entities;

namespace Teams.Core.Services.Jobs.Responses;

public record JobModel(
    string Id,
    string IdempotencyKey,
    string ConcurrencyToken,
    string Type,
    string Status,
    string? Parameters,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime DateCreated,
    DateTime DateModified)
{
    public static JobModel FromEntity(Job job)
        => new(
            Id: job.Id,
            IdempotencyKey: job.IdempotencyKey,
            ConcurrencyToken: job.ConcurrencyToken,
            Type: job.Type.ToString(),
            Status: job.Status.ToString(),
            Parameters: job.Parameters,
            ErrorCode: job.ErrorCode,
            ErrorMessage: job.ErrorMessage,
            DateCreated: job.DateCreated,
            DateModified: job.DateModified);
};