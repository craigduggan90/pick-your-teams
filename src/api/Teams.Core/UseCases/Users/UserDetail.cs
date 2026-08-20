using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users;

public record UserDetail(User User, int PendingInvitations);
