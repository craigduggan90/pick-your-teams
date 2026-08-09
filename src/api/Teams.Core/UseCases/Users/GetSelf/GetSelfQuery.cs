using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetSelf;

public record GetSelfQuery() : IRequest<User>;