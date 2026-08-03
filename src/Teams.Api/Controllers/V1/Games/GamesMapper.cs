using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Games.CreateGame;
using Teams.Core.UseCases.Games.GetGames;
using Teams.Core.UseCases.Games.UpdateGame;
using Teams.Domain.Entities;

namespace Teams.Api.Controllers.V1.Games;

public static class GamesMapper
{
    public static GameModel ToModel(this Game game) => new(
        Id: game.Id,
        Location: game.Location,
        StartTime: game.StartTime,
        Duration: game.Duration,
        TeamSize: game.TeamSize,
        Status: game.Status.ToString());

    public static GameDetailModel ToDetailedModel(this Game game) => new(
        Id: game.Id,
        Location: game.Location,
        StartTime: game.StartTime,
        Duration: game.Duration,
        TeamSize: game.TeamSize,
        Status: game.Status.ToString(),
        Winner: game.Winner?.ToString(),
        HomeTeamRating: game.HomeTeamRating,
        AwayTeamRating: game.AwayTeamRating,
        game.DateCreated,
        game.DateModified);

    public static CreateGameCommand ToCommand(this CreateGameRequestModel model) => new(
        Location: model.Location,
        StartTime: model.StartTime,
        Duration: model.Duration,
        TeamSize: model.TeamSize,
        OrganiserId: model.OrganiserId);

    public static UpdateGameCommand ToCommand(this UpdateGameRequestModel model, string id) => new(
        Id: id,
        Location: model.Location,
        StartTime: model.StartTime,
        Duration: model.Duration);

    public static GetGamesQuery ToQuery(this GetGamesRequestModel model) => new(
        model.Location,
        model.StartTimeFrom,
        model.StartTimeTo,
        model.DurationFrom,
        model.DurationTo,
        model.TeamSize,
        model.CreatedFrom,
        model.CreatedTo,
        model.ModifiedFrom,
        model.ModifiedTo,
        model.PageSize,
        model.Cursor.TryDecodeCursor(out var c) ? c : null);
}