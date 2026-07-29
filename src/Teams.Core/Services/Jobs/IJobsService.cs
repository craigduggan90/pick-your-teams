using Teams.Common.Pagination;
using Teams.Core.Services.Jobs.Requests;
using Teams.Core.Services.Jobs.Responses;

namespace Teams.Core.Services.Jobs;

public interface IJobsService
{
    Task<PagedList<JobModel>> GetJobsAsync(GetJobsRequest request, CancellationToken cancellationToken);

    Task<JobModel> GetJobByIdAsync(string id, CancellationToken cancellationToken);

    Task<JobModel> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken);

    Task<JobModel> UpdateJobAsync(UpdateJobRequest request, CancellationToken cancellationToken);
}