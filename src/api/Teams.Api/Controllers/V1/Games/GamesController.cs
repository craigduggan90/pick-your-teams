using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Games.Examples;
using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Api.Controllers.V1.Shared;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Common.Pagination;
using Teams.Core.CQRS;
using Teams.Core.UseCases.Games.DeleteGame;
using Teams.Core.UseCases.Games.GetGameById;
using Teams.Core.UseCases.Games.SetTeams;
using Teams.Domain.Extensions;

namespace Teams.Api.Controllers.V1.Games;

public class GamesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<GameModel>>(200)]
    [SwaggerResponseExample(200, typeof(GameModelPageExample))]
    public async Task<IActionResult> GetGames([FromQuery] GetGamesRequestModel query, CancellationToken cancellationToken)
    {
        var entities = await mediator.SendAsync(query.ToQuery(), cancellationToken);
        return Ok(entities.ToPagedList(GamesMapper.ToModel));
    }

    [HttpPost]
    [ProducesResponseType<GameModel>(201)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(CreateGameRequestModel), typeof(CreateGameRequestModelExample))]
    [SwaggerResponseExample(200, typeof(GameModelExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreateGame(
        [FromBody] CreateGameRequestModel body,
        CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(body.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetGameById), new { id = entity.Id }, entity.ToModel());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<GameDetailModelExample>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerRequestExample(typeof(UpdateGameRequestModel), typeof(UpdateGameRequestModelExample))]
    [SwaggerResponseExample(200, typeof(GameDetailModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetGameById(
        string id,
        CancellationToken cancellationToken)
    {
        var entity = await mediator.SendAsync(new GetGameByIdQuery(id), cancellationToken);
        return Ok(entity.ToDetailedModel());
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(UpdateGameRequestModel), typeof(UpdateGameRequestModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> UpdateGame(
        string id,
        [FromBody] UpdateGameRequestModel body,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(body.ToCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    public async Task<IActionResult> DeleteGame(
        string id,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(new DeleteGameCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/teams/generate")]
    [ProducesResponseType<GameTeamsModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(GenerateTeamsRequestModel), typeof(GenerateTeamsRequestModelExample))]
    [SwaggerResponseExample(200, typeof(GameTeamsModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> GenerateTeams(
        string id,
        [FromBody] GenerateTeamsRequestModel body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(body.ToCommand(id), cancellationToken);
        return result.Count == 0
            ? Ok()
            : Ok(result.FirstOrDefault().ToModel(id));
    }

    [HttpGet("{id}/teams")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(200, typeof(GameTeamsModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetTeams(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetGameByIdQuery(id), cancellationToken);
        return Ok(result.ToTeamsModel());
    }

    [HttpPut("{id}/teams")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(SetTeamsRequestModel), typeof(SetTeamsRequestModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> SetTeams(
        string id,
        [FromBody] SetTeamsRequestModel body,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(body.ToCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/teams")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerRequestExample(typeof(SetTeamsRequestModel), typeof(SetTeamsRequestModelExample))]
    [SwaggerResponseExample(404, typeof(GameNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> ClearTeams(
        string id,
        CancellationToken cancellationToken)
    {
        _ = await mediator.SendAsync(new SetTeamsCommand(id, [], []), cancellationToken);
        return NoContent();
    }
}