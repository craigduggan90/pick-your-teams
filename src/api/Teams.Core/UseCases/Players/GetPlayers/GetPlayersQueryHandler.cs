using Teams.Core.CQRS;
using Teams.Data.Models;
using Teams.Data.Repositories.Players;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Players.GetPlayers;

public class GetPlayersQueryHandler(IReadOnlyPlayersRepository repository)
    : IRequestHandler<GetPlayersQuery, IReadOnlyCollection<Player>>
{
    public async Task<IReadOnlyCollection<Player>> HandleAsync(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAsync(
            request.GameId,
            request.DisplayName,
            request.UserId,
            new RangeFilter<int>(request.RatingFrom, request.RatingTo),
            request.Team,
            request.Type,
            new DateFilter(
                new RangeFilter<DateTime>(request.CreatedFrom, request.CreatedTo),
                new RangeFilter<DateTime>(request.ModifiedFrom, request.ModifiedTo)),
            new PaginationFilter(request.Cursor, request.PageSize),
            cancellationToken);

        return [.. entities];
    }
}