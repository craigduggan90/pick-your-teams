using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Players.Examples;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Api.Controllers.V1.Shared;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Common.Pagination;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Players.DeletePlayer;
using Teams.Core.UseCases.Players.GetPlayerById;
using Teams.Domain.Extensions;

namespace Teams.Api.Controllers.V1.Players;

public class PlayersController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<PlayerModel>>(200)]
    [SwaggerResponseExample(200, typeof(PlayerModelPageExample))]
    public async Task<IActionResult> GetPlayers(
        [FromQuery] GetPlayersRequestModel request,
        CancellationToken cancellationToken)
    {
        var entities = await mediator.SendAsync(request.ToQuery(), cancellationToken);
        return Ok(entities.ToPagedList(PlayersMapper.ToPlayerModel));
    }

    [HttpPost]
    [ProducesResponseType<PlayerModel>(201)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreatePlayerRequestModel), typeof(CreatePlayerRequestModelExample))]
    [SwaggerResponseExample(201, typeof(PlayerModelExample))]
    [SwaggerResponseExample(404, typeof(UserNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreatePlayer(
        [FromBody] CreatePlayerRequestModel body,
        CancellationToken cancellationToken)
    {
        var player = await mediator.SendAsync(body.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetPlayerById), new { id = player.Id }, player.ToPlayerModel());
    }

    [HttpPost("dummy")]
    [ProducesResponseType<PlayerModel>(201)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreateDummyPlayerRequestModel), typeof(CreateDummyPlayerRequestModelExample))]
    [SwaggerResponseExample(201, typeof(PlayerModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreateDummyPlayer(
        [FromBody] CreateDummyPlayerRequestModel body,
        CancellationToken cancellationToken)
    {
        var player = await mediator.SendAsync(body.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetPlayerById), new { id = player.Id }, player.ToPlayerModel());
    }

    [HttpGet("id")]
    [ProducesResponseType<PlayerDetailModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(PlayerDetailModelExample))]
    [SwaggerResponseExample(404, typeof(PlayerNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetPlayerById(string id, CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(new GetPlayerByIdQuery(id), cancellationToken);
        return Ok(entity.ToPlayerDetailModel());
    }

    [HttpDelete("id")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(404, typeof(PlayerNotFoundProblemDetailsExample))]
    public async Task<IActionResult> DeletePlayer(string id, CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(new DeletePlayerCommand(id), cancellationToken);
        return NoContent();
    }
}