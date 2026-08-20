using Teams.Core.CQRS;

namespace Teams.Core.UseCases.Users.GetSelf;

public record GetSelfQuery() : IRequest<UserDetail>;