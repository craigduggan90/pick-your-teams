using Teams.Core.CQRS;
using Teams.Data.Models;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.GetGames;

public class GetGamesQueryHandler(IReadOnlyGamesRepository repository)
    : IRequestHandler<GetGamesQuery, IReadOnlyCollection<Game>>
{
    public async Task<IReadOnlyCollection<Game>> HandleAsync(GetGamesQuery request, CancellationToken cancellationToken)
    {
        var games = await repository.GetAsync(
            location: request.Location,
            startTime: new RangeFilter<DateTime>(request.StartTimeFrom, request.StartTimeTo),
            duration: new RangeFilter<int>(request.DurationFrom, request.DurationTo),
            teamSize: request.TeamSize,
            status: request.Status,
            dateFilter: new DateFilter(
                new RangeFilter<DateTime>(request.CreatedFrom, request.CreatedTo),
                new RangeFilter<DateTime>(request.ModifiedFrom, request.ModifiedTo)),
            pagination: new PaginationFilter(request.Cursor, request.PageSize),
            cancellationToken: cancellationToken);

        return [.. games];
    }
}