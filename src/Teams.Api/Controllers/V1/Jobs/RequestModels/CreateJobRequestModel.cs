using System.Text.Json;

namespace Teams.Api.Controllers.V1.Jobs.RequestModels;

public record CreateJobRequestModel(string Type, JsonElement? Parameters);