using Teams.Common.Extensions;
using Teams.Domain.Entities.Abstract;
using Teams.Domain.Enums;

namespace Teams.Domain.Entities;

public class Job : EntityBase
{
    public Job(string idempotencyKey, JobTypeEnum type, string? parameters)
    {
        IdempotencyKey = idempotencyKey;
        Type = type;
        Parameters = parameters;
        ConcurrencyToken = GetConcurrencyToken();
    }

    public string IdempotencyKey { get; }

    public JobTypeEnum Type { get; }

    public JobStatusEnum Status { get; private set; } = JobStatusEnum.Pending;

    public string? Parameters { get; }

    public string? ErrorCode { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string ConcurrencyToken { get; private set; }

    public override object AsSerializable()
        => new { Id, IdempotencyKey, Type, Status, ErrorCode, DateCreated, DateModified };

    public void Update(
        JobStatusEnum status,
        string? errorCode,
        string? errorMessage)
    {
        UpdateProperty(nameof(Status), status);
        UpdateProperty(nameof(ErrorCode), errorCode);
        UpdateProperty(nameof(ErrorMessage), errorMessage);
    }

    public void Delete()
    {
        if (DateDeleted.HasValue)
            return;

        SetDateModified();
        SoftDelete();
    }

    protected override void SetDateModified()
    {
        base.SetDateModified();
        ConcurrencyToken = GetConcurrencyToken();
    }

    private string GetConcurrencyToken() => DateModified.ToString("O").GetMd5Digest();
}