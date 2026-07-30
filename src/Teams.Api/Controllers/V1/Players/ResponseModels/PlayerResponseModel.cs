namespace Teams.Api.Controllers.V1.Players.ResponseModels;

public record PlayerResponseModel(
    string Id,
    string Name,
    int Rating);