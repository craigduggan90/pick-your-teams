using Teams.Api.Controllers.V1.Players.RequestModels;
using Teams.Api.Controllers.V1.Players.ResponseModel;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Players.CreateDummyPlayer;
using Teams.Core.UseCases.Players.CreatePlayer;
using Teams.Core.UseCases.Players.GetPlayers;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Api.Controllers.V1.Players;

public static class PlayersMapper
{
    public static PlayerModel ToPlayerModel(this Player entity) =>
        new(Id: entity.Id,
            GameId: entity.GameId,
            UserId: entity.UserId,
            Tag: entity.User?.Tag,
            Type: entity.Type.ToString(),
            DisplayName: entity.GetDisplayName(),
            Rating: entity.Rating,
            Team: entity.Team.ToString());

    public static PlayerDetailModel ToPlayerDetailModel(this Player entity) =>
        new(Id: entity.Id,
            GameId: entity.GameId,
            UserId: entity.UserId,
            Tag: entity.User?.Tag,
            Type: entity.Type.ToString(),
            DisplayName: entity.GetDisplayName(),
            Rating: entity.Rating,
            RatingChange: entity.RatingChange,
            Team: entity.Team.ToString(),
            Created: entity.DateCreated,
            Modified: entity.DateModified);

    public static GetPlayersQuery ToQuery(this GetPlayersRequestModel request) =>
        new(GameId: request.GameId,
            DisplayName: request.DisplayName,
            UserId: request.UserId,
            RatingFrom: request.RatingFrom,
            RatingTo: request.RatingTo,
            Team: Enum.TryParse<GameTeamEnum>(request.Team, true, out var team) ? team : null,
            Type: Enum.TryParse<PlayerTypeEnum>(request.Type, true, out var type) ? type : null,
            CreatedFrom: request.CreatedFrom,
            CreatedTo: request.CreatedTo,
            ModifiedFrom: request.ModifiedFrom,
            ModifiedTo: request.ModifiedTo,
            PageSize: request.PageSize,
            Cursor: request.Cursor.TryDecodeCursor(out var cursor) ? cursor : null);

    public static CreatePlayerCommand ToCommand(this CreatePlayerRequestModel request) =>
        new(request.GameId, request.UserId);

    public static CreateDummyPlayerCommand ToCommand(this CreateDummyPlayerRequestModel request) =>
        new(request.GameId, request.DisplayName, request.EstimatedRating);
}