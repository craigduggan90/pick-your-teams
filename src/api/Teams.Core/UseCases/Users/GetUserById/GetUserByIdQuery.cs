using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUserById;

public record GetUserByIdQuery(string Id) : IRequest<User>;