using Teams.Core.CQRS;
using Teams.Core.UseCases.Users;

namespace Teams.Core.UseCases.Users.GetUserById;

public record GetUserByIdQuery(string Id) : IRequest<UserDetail>;