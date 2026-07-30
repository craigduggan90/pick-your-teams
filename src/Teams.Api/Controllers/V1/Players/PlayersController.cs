using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Controllers.V1.Abstract;
using Teams.Api.Controllers.V1.Players.Examples;
using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Api.Controllers.V1.Players.ResponseModels;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;
using Teams.Api.Infrastructure.Validation;
using Teams.Common.Extensions;
using Teams.Common.Pagination;
using Teams.Core.Services.Players;
using Teams.Core.Services.Players.Commands;
using Teams.Core.Services.Players.Queries;

namespace Teams.Api.Controllers.V1.Players;

public class PlayersController(
    IPlayersService service,
    IValidationService validators) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedList<PlayerResponseModel>>(200)]
    [ProducesResponseType<ProblemDetails>(400)]
    [SwaggerResponseExample(200, typeof(PlayerResponseModelPageExample))]
    [SwaggerResponseExample(400, typeof(QueryValidationProblemDetailsExample))]
    public async Task<IActionResult> GetPlayers(
        [FromQuery] GetPlayersRequestModel request,
        CancellationToken cancellationToken = default)
    {
        await validators.ValidateQueryAsync(request, cancellationToken);

        var query = new GetPlayersQuery(
            Name: request.Name,
            RatingFrom: request.RatingFrom,
            RatingTo: request.RatingTo,
            CreatedFrom: request.CreatedFrom,
            CreatedTo: request.CreatedTo,
            ModifiedFrom: request.ModifiedFrom,
            ModifiedTo: request.ModifiedTo,
            Cursor: request.Cursor.TryDecodeCursor(out var cursor) ? cursor : null,
            PageSize: request.PageSize);

        var result = await service.GetPlayersAsync(query, cancellationToken);
        return Ok(result.Map(PlayersMapper.ToPlayerResponseModel));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PlayerDetailResponseModel>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(201, typeof(PlayerDetailResponseModelExample))]
    [SwaggerResponseExample(404, typeof(PlayerNotFoundProblemDetailsExample))]
    public async Task<IActionResult> GetPlayerById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var player = await service.GetPlayerByIdAsync(new GetPlayerByIdQuery(id), cancellationToken);
        return Ok(player.ToPlayerDetailResponseModel());
    }

    [HttpPost]
    [SwaggerRequestExample(typeof(CreatePlayerRequestModel), typeof(CreatePlayerRequestModelExample))]
    [ProducesResponseType<PlayerResponseModel>(201)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerResponseExample(201, typeof(PlayerResponseModel))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> CreatePlayer(
        [FromBody] CreatePlayerRequestModel request,
        CancellationToken cancellationToken = default)
    {
        await validators.ValidateCommandAsync(request, cancellationToken);

        var command = new CreatePlayerCommand(request.Name, request.UserId);
        var created = await service.CreatePlayerAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetPlayerById),
            new { id = created.Id },
            created.ToPlayerResponseModel());
    }

    [HttpPatch("{id}")]
    [SwaggerRequestExample(typeof(UpdatePlayerRequestModel), typeof(UpdatePlayerRequestModelExample))]
    [ProducesResponseType<PlayerResponseModel>(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    [SwaggerResponseExample(404, typeof(PlayerNotFoundProblemDetailsExample))]
    [SwaggerResponseExample(422, typeof(CommandValidationProblemDetailsExample))]
    public async Task<IActionResult> UpdatePlayer(
        string id,
        [FromBody] UpdatePlayerRequestModel request,
        CancellationToken cancellationToken = default)
    {
        await validators.ValidateCommandAsync(request, cancellationToken);

        var command = new UpdatePlayerCommand(id, request.Name);
        await service.UpdatePlayerAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<PlayerResponseModel>(204)]
    [ProducesResponseType<ProblemDetails>(404)]
    [SwaggerResponseExample(404, typeof(PlayerNotFoundProblemDetailsExample))]
    public async Task<IActionResult> DeletePlayer(
        string id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeletePlayerCommand(id);
        await service.DeletePlayerAsync(command, cancellationToken);

        return NoContent();
    }
}