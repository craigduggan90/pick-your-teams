namespace Teams.Api.Controllers.V1.Players.ResponseModels;

public record PlayerDetailResponseModel(
    string Id,
    string Name,
    int Rating,
    DateTime DateCreated,
    DateTime DateModified);