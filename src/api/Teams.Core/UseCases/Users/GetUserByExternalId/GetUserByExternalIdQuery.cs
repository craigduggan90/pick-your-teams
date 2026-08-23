using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUserByExternalId;

public record GetUserByExternalIdQuery(string ExternalId) : IRequest<User>;