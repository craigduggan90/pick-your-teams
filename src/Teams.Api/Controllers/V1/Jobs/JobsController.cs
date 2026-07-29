using Microsoft.AspNetCore.Mvc;
using Teams.Api.Attributes;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Jobs.RequestModels;
using Teams.Api.Controllers.V1.Jobs.ResponseModels;
using Teams.Api.Infrastructure;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Jobs;
using Teams.Common;
using Teams.Common.Extensions;
using Teams.Common.Pagination;
using Teams.Core.Services.Jobs;
using Swashbuckle.AspNetCore.Filters;

namespace Teams.Api.Controllers.V1.Jobs;

public class JobsController(IJobsService jobsService) : ApiControllerBase
{
    [HttpGet]
    [RequiresScope(Scopes.Jobs.Read)]
    [ProducesResponseType<PagedList<JobResponseModel>>(200)]
    [ProducesResponseType<ProblemDetails>(400)]
    [SwaggerResponseExample(200, typeof(JobResponseModelPageExample))]
    [SwaggerResponseExample(400, typeof(QueryValidationProblemDetailsExample))]
    public async Task<IActionResult> GetJobs(
        [FromQuery] GetJobsRequestModel query,
        CancellationToken cancellationToken)
    {
        var result = await jobsService.GetJobsAsync(query.ToGetJobsRequestRequest(), cancellationToken);
        return Ok(result.Map(JobsMapper.ToJobResponseModel));
    }

    [HttpGet("{id}")]
    [RequiresScope(Scopes.Jobs.Read)]
    [ProducesResponseType<JobResponseDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(JobResponseDetailModelExample))]
    [SwaggerResponseExample(404, typeof(JobNotFoundProblemDetailsFactory))]
    public async Task<IActionResult> GetJobById(
        string id,
        CancellationToken cancellationToken)
    {
        var job = await jobsService.GetJobByIdAsync(id, cancellationToken);
        SetEtagResponseHeader(job.ConcurrencyToken);
        return Ok(job.ToJobResponseDetailModel());
    }

    [HttpPost]
    [RequiresScope(Scopes.Jobs.Enqueue)]
    [RequiresHeader(Constants.IdempotencyHeaderKey)]
    [SwaggerRequestExample(typeof(CreateJobRequestModel), typeof(CreateJobRequestModelExample))]
    [ProducesResponseType<JobResponseModel>(202)]
    [ProducesResponseType<ProblemDetails>(422)]
    [ProducesResponseType<ProblemDetails>(428)]
    [SwaggerResponseExample(202, typeof(JobResponseModelExample))]
    [SwaggerResponseExample(422, typeof(JobResponseModelExample))]
    [SwaggerResponseExample(428, typeof(MissingIdempotencyKeyExample))]
    public async Task<IActionResult> CreateJob(
        [FromBody] CreateJobRequestModel request,
        [FromHeader(Name = Constants.IdempotencyHeaderKey)] string? idempotency,
        CancellationToken cancellationToken)
    {
        var model = request.ToCreateJobRequest(idempotency);
        var job = await jobsService.CreateJobAsync(model, cancellationToken);
        SetEtagResponseHeader(job.ConcurrencyToken);
        return AcceptedAtAction(
            nameof(GetJobById),
            new
            {
                id = job.Id,
                version = HttpContext.RequestedApiVersion?.ToString()
            },
            job.ToJobResponseModel());
    }

    [HttpPut("{id}")]
    [RequiresScope(Scopes.Jobs.Modify)]
    [RequiresHeader(Constants.IfMatchHeaderKey)]
    [ProducesResponseType<JobResponseModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(412)]
    [ProducesResponseType<ProblemDetails>(428)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerResponseExample(200, typeof(JobResponseModelExample))]
    [SwaggerResponseExample(404, typeof(JobNotFoundProblemDetailsFactory))]
    [SwaggerResponseExample(421, typeof(ConcurrencyTokenMismatchExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    [SwaggerResponseExample(428, typeof(MissingConcurrencyTokenExample))]
    public async Task<IActionResult> UpdateJob(
        [FromRoute] string id,
        [FromHeader(Name = Constants.IfMatchHeaderKey)] string? concurrencyToken,
        [FromBody] UpdateJobRequestModel request,
        CancellationToken cancellationToken)
    {
        var model = request.ToUpdateJobRequest(id, concurrencyToken);
        var job = await jobsService.UpdateJobAsync(model, cancellationToken);
        SetEtagResponseHeader(job.ConcurrencyToken);
        return Ok(job.ToJobResponseModel());
    }
}