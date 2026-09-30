using Beans.Requestable;

namespace Teams.Core.UseCases.Users.GetSelf;

public record GetSelfQuery() : IRequest<UserDetail>;