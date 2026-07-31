using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Players;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.GetPlayerById;

public class GetPlayerByIdQueryHandler(IReadOnlyPlayersRepository repository)
    : IRequestHandler<GetPlayerByIdQuery, Player>
{
    public async Task<Player> HandleAsync(GetPlayerByIdQuery request, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(request.Id, cancellationToken)
        ?? throw new NotFoundException(typeof(Player), request.Id);
}