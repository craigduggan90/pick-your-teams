namespace Teams.Api.Controllers.V1.Users.RequestModels;

public record GetUsersRequestModel(
    string? EmailAddress = null,
    string? Tag = null,
    string? DisplayName = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    string? Cursor = null);