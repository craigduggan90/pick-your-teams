using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Games;

/// <summary>Describes a read-write repository containing instances of <see cref="Game"/>.</summary>
public interface IGamesRepository : IReadWriteRepository<Game>, IReadOnlyGamesRepository;