using Beans.Requestable;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.DeleteUser;

public record DeleteUserCommand(string Id) : IRequest<User>;