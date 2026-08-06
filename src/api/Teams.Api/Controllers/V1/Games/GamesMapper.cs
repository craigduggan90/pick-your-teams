using Teams.Api.Controllers.V1.Games.RequestModels;
using Teams.Api.Controllers.V1.Games.ResponseModels;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Games.CreateGame;
using Teams.Core.UseCases.Games.GenerateTeams;
using Teams.Core.UseCases.Games.GetGames;
using Teams.Core.UseCases.Games.SetTeams;
using Teams.Core.UseCases.Games.UpdateGame;
using Teams.Domain.Entities;
using Teams.Domain.Enums;
using Teams.Domain.Models;

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

    public static GameTeamsModel ToTeamsModel(this Game game) => new(
        Id: game.Id,
        Home: new GameTeamModel(
            Players: [.. game.Players.Where(player => player.Team == GameTeamEnum.Home).Select(ToGameTeamPlayerModel)],
            TeamRating: game.HomeTeamRating ?? 0),
        Away: new GameTeamModel(
            Players: [.. game.Players.Where(player => player.Team == GameTeamEnum.Away).Select(ToGameTeamPlayerModel)],
            TeamRating: game.AwayTeamRating ?? 0));

    public static GameTeamsModel ToModel(this TeamSuggestion teams, string gameId) => new(
            Id: gameId,
            Home: new GameTeamModel(
                Players: [.. teams.Home.Select(ToGameTeamPlayerModel)],
                TeamRating: teams.HomeRating
            ),
            Away: new GameTeamModel(
                Players: [.. teams.Away.Select(ToGameTeamPlayerModel)],
                TeamRating: teams.AwayRating
            )
        );

    public static GameTeamPlayerModel ToGameTeamPlayerModel(this Player player) => new(
        player.Id,
        player.DisplayName,
        player.Rating);

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

    public static GenerateTeamsCommand ToCommand(this GenerateTeamsRequestModel model, string id) => new(
        GameId: id,
        HomeSeedPlayerIds: model.HomeTeamSeedIds.ToList(),
        AwaySeedPlayerIds: model.AwayTeamSeedIds.ToList(),
        Differential: model.Differential,
        Count: 1);

    public static SetTeamsCommand ToCommand(this SetTeamsRequestModel model, string id) => new(
        GameId: id,
        HomeTeamIds: model.HomeTeamIds,
        AwayTeamIds: model.AwayTeamIds);

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