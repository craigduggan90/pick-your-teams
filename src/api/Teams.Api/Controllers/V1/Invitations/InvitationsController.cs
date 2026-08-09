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

namespace Teams.Api.Controllers.V1.Invitations;

public class InvitationsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<InvitationModel>>(200)]
    [SwaggerResponseExample(200, typeof(InvitationModelPageExample))]
    public async Task<IActionResult> GetInvitations([FromQuery] GetInvitationsRequestModel query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreateInvitationsRequestModel), typeof(CreateInvitationsRequestModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreateInvitations(
        [FromBody] CreateInvitationsRequestModel body,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    [ProducesResponseType<InvitationDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(InvitationDetailModelExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetInvitationById(
        string id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpPost("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422, Description = "")]
    [SwaggerRequestExample(typeof(CreateInvitationsRequestModel), typeof(CreateInvitationsRequestModelExample))]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    public async Task<IActionResult> AcceptInvitation(
        string id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(InvitationNotFoundProblemDetailsExample))]
    public async Task<IActionResult> DeclineInvitation(
        string id,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}