using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Users;

/// <summary>Describes a read-only repository containing instances of <see cref="User"/>.</summary>
public interface IReadOnlyUsersRepository : IReadOnlyRepository<User>
{
}