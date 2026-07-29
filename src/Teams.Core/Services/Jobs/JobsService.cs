using Teams.Common.Pagination;
using Teams.Core.Extensions;
using Teams.Data.Models;
using Teams.Data.Repositories.Jobs;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Core.Exceptions;
using Teams.Core.Services.Jobs.Requests;
using Teams.Core.Services.Jobs.Responses;
using Teams.Core.Services.Validation;

namespace Teams.Core.Services.Jobs;

public class JobsService(IReadOnlyJobsRepository repository, IUnitOfWork unitOfWork, IValidationService validator)
    : IJobsService
{
    public async Task<PagedList<JobModel>> GetJobsAsync(GetJobsRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateQueryAsync(request, cancellationToken);

        JobTypeEnum? type = request.Type is not null ? Enum.Parse<JobTypeEnum>(request.Type, true) : null;
        JobStatusEnum? status = request.Status is not null ? Enum.Parse<JobStatusEnum>(request.Status, true) : null;
        request.Cursor.TryDecodeCursor(out var cursor);

        var jobs = await repository.GetAsync(
            type,
            status,
            request.ErrorCode,
            new DateFilter(request.CreatedFrom, request.CreatedTo, request.ModifiedFrom, request.ModifiedTo),
            new PaginationFilter(cursor, request.PageSize),
            cancellationToken);

        return jobs.ToList().ToPagedList(JobModel.FromEntity);
    }

    public async Task<JobModel> GetJobByIdAsync(string id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken) is { } job
            ? JobModel.FromEntity(job)
            : throw new NotFoundException(typeof(Job), id);

    public async Task<JobModel> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateCommandAsync(request, cancellationToken);

        if (await unitOfWork.Jobs.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken) is { } extant)
            return JobModel.FromEntity(extant);

        var type = Enum.Parse<JobTypeEnum>(request.Type, true);
        var job = new Job(request.IdempotencyKey, type, request.Parameters);

        var created = await unitOfWork.Jobs.CreateAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return JobModel.FromEntity(created);
    }

    public async Task<JobModel> UpdateJobAsync(UpdateJobRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateCommandAsync(request, cancellationToken);

        var job = await unitOfWork.Jobs.GetByIdAsync(request.Id, cancellationToken) ??
                  throw new NotFoundException(typeof(Job), request.Id);

        ConcurrencyTokenMismatchException.ThrowIfMismatch(request.ConcurrencyToken, job.ConcurrencyToken);

        var status = Enum.Parse<JobStatusEnum>(request.Status, true);
        job.Update(status, request.ErrorCode, request.ErrorMessage);

        if (!job.IsDirty)
            return JobModel.FromEntity(job);

        await unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return JobModel.FromEntity(job);
    }
}