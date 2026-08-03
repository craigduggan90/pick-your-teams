using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.CreateUser;

public record CreateUserCommand(string DisplayName, string ExternalId, string Email, string? Mobile) : IRequest<User>;