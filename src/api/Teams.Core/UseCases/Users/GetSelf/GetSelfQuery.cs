using Teams.Core.CQRS;
using Teams.Core.UseCases.Users;

namespace Teams.Core.UseCases.Users.GetSelf;

public record GetSelfQuery() : IRequest<UserDetail>;