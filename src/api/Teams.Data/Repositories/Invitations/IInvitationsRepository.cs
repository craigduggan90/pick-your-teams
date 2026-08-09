using Teams.Domain.Entities;

namespace Teams.Data.Repositories.Invitations;

public interface IInvitationsRepository : IReadWriteRepository<Invitation>, IReadOnlyInvitationsRepository;