using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.UpdateUser;

public record UpdateUserCommand(string Id, string? Tag, string? DisplayName, string? Email, string? Mobile)
    : IRequest<User>;