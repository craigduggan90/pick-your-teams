using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Users.CreateUser;
using Teams.Core.UseCases.Users.GetUsers;
using Teams.Core.UseCases.Users.UpdateUser;
using Teams.Domain.Entities;

namespace Teams.Api.Controllers.V1.Users;

public static class UsersMapper
{
    public static UserModel ToModel(this User user) => new(
        Id: user.Id,
        Tag: user.Tag,
        DisplayName: user.DisplayName,
        Rating: user.Rating);

    public static UserDetailModel ToDetailedModel(this User user) => new(
        Id: user.Id,
        Tag: user.Tag,
        DisplayName: user.DisplayName,
        Rating: user.Rating,
        Email: user.EmailAddress,
        Mobile: user.Mobile,
        Created: user.DateCreated,
        Modified: user.DateModified);

    public static CreateUserCommand ToCommand(this CreateUserRequestModel model) => new(
        DisplayName: model.DisplayName,
        ExternalId: model.ExternalId,
        Email: model.Email,
        Mobile: model.Mobile);

    public static UpdateUserCommand ToCommand(this UpdateUserRequestModel model, string id) => new(
        Id: id,
        Tag: model.Tag,
        DisplayName: model.DisplayName,
        Email: model.Email,
        Mobile: model.Mobile);

    public static GetUsersQuery ToQuery(this GetUsersRequestModel model) => new(
        model.EmailAddress,
        model.Tag,
        model.DisplayName,
        model.RatingFrom,
        model.RatingTo,
        model.CreatedFrom,
        model.CreatedTo,
        model.ModifiedFrom,
        model.ModifiedTo,
        model.PageSize,
        model.Cursor.TryDecodeCursor(out var c) ? c : null);
}