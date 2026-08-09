using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Invitations.Examples;
using Teams.Api.Controllers.V1.Invitations.RequestModels;
using Teams.Api.Controllers.V1.Invitations.ResponseModels;
using Teams.Api.Controllers.V1.Shared;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Common.Pagination;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Invitations.AcceptInvitation;
using Teams.Core.UseCases.Invitations.DeclineInvitation;
using Teams.Core.UseCases.Invitations.GetInvitationById;
using Teams.Domain.Extensions;

namespace Teams.Api.Controllers.V1.Invitations;

public class InvitationsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<InvitationModel>>(200)]
    [ProducesResponseType<ProblemDetails>(403)]
    [SwaggerResponseExample(200, typeof(InvitationModelPageExample))]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    public async Task<IActionResult> GetInvitations([FromQuery] GetInvitationsRequestModel query, CancellationToken cancellationToken)
    {
        var entities = await mediator.SendAsync(query.ToQuery(), cancellationToken);
        return Ok(entities.ToPagedList(InvitationsMapper.ToModel));
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreateInvitationsRequestModel), typeof(CreateInvitationsRequestModelExample))]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreateInvitations(
        [FromBody] CreateInvitationsRequestModel body,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(body.ToCommand(), cancellationToken);
        return StatusCode(201);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<InvitationDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(InvitationDetailModelExample))]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetInvitationById(
        string id,
        CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(new GetInvitationByIdQuery(id), cancellationToken);
        return Ok(entity.ToDetailModel());
    }

    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> AcceptInvitation(
        string id,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new AcceptInvitationCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> DeclineInvitation(
        string id,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeclineInvitationCommand(id), cancellationToken);
        return NoContent();
    }
}