using Teams.Api.Controllers.V1.Users.RequestModels;
using Teams.Api.Controllers.V1.Users.ResponseModels;
using Teams.Common.Pagination;
using Teams.Core.UseCases.Users;
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

    public static UserDetailModel ToDetailedModel(this UserDetail detail) => new(
        Id: detail.User.Id,
        Tag: detail.User.Tag,
        DisplayName: detail.User.DisplayName,
        Rating: detail.User.Rating,
        Email: detail.User.EmailAddress,
        Mobile: detail.User.Mobile,
        Created: detail.User.DateCreated,
        Modified: detail.User.DateModified,
        PendingInvitations: detail.PendingInvitations);

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