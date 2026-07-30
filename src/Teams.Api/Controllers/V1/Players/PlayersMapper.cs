using Teams.Api.Controllers.V1.Players.ResponseModels;
using Teams.Common.Pagination;
using Teams.Domain.Entities;

namespace Teams.Api.Controllers.V1.Players;

internal static class PlayersMapper
{
    public static PagedList<PlayerResponseModel> ToPlayerResponseModels(this PagedList<Player> collection)
        => new(collection.Data.Select(ToPlayerResponseModel).ToList(), collection.Cursor, collection.Count);

    public static PlayerResponseModel ToPlayerResponseModel(this Player player)
        => new(player.Id, player.Name, player.Rating);

    public static PlayerDetailResponseModel ToPlayerDetailResponseModel(this Player player)
        => new(player.Id, player.Name, player.Rating, player.DateCreated, player.DateModified);
}