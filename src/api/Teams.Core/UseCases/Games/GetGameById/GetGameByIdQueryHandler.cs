using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.GetGameById;

public class GetGameByIdQueryHandler(IReadOnlyGamesRepository repository) : IRequestHandler<GetGameByIdQuery, Game>
{
    public async Task<Game> HandleAsync(GetGameByIdQuery request, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(request.Id, cancellationToken)
        ?? throw new NotFoundException(typeof(Game), request.Id);
}