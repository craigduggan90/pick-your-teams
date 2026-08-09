using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Shared;
using Teams.Api.Controllers.V1.Users.Examples;
using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Common.Pagination;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Users.DeleteUser;
using Teams.Core.UseCases.Users.GetSelf;
using Teams.Core.UseCases.Users.GetUserById;
using Teams.Domain.Extensions;

namespace Teams.Api.Controllers.V1.Users;

public class UsersController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<UserModel>>(200)]
    [SwaggerResponseExample(200, typeof(UserModelPageExample))]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequestModel query, CancellationToken cancellationToken)
    {
        var entities = await mediator.SendAsync(query.ToQuery(), cancellationToken);
        return Ok(entities.ToPagedList(UsersMapper.ToModel));
    }

    [HttpPost]
    [ProducesResponseType<UserModel>(201)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreateUserRequestModel), typeof(CreateUserRequestModelExample))]
    [SwaggerResponseExample(201, typeof(UserModelExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequestModel body,
        CancellationToken cancellationToken)
    {
        // Note: intentionally not actor-checked - this is the IdP-triggered create call, made
        // before any User exists to resolve the caller's identity against.
        var entity = await mediator.SendAsync(body.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = entity.Id }, entity.ToModel());
    }

    [HttpGet("self")]
    [ProducesResponseType<UserDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(UserDetailModelExample))]
    [SwaggerResponseExample(404, typeof(UserNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetSelf(CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(new GetSelfQuery(), cancellationToken);
        return Ok(entity.ToDetailedModel());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<UserDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(UserDetailModelExample))]
    [SwaggerResponseExample(404, typeof(UserNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetUserById(
        string id,
        CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(new GetUserByIdQuery(id), cancellationToken);
        return Ok(entity.ToDetailedModel());
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(UpdateUserRequestModel), typeof(UpdateUserRequestModelExample))]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(UserNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> UpdateUser(
        string id,
        [FromBody] UpdateUserRequestModel body,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(body.ToCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(403, typeof(AccessDeniedProblemDetailsExample))]
    [SwaggerResponseExample(404, typeof(UserNotFoundProblemDetailsExample))]
    public async Task<IActionResult> DeleteUser(
        string id,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}