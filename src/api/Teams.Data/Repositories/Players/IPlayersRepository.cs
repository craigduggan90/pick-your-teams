using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Players;

/// <summary>Describes a read-write repository containing instances of <see cref="Player"/>.</summary>
public interface IPlayersRepository : IReadWriteRepository<Player>, IReadOnlyPlayersRepository;