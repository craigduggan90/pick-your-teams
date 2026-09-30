using Beans.Requestable;

namespace Teams.Core.UseCases.Users.GetUserById;

public record GetUserByIdQuery(string Id) : IRequest<UserDetail>;