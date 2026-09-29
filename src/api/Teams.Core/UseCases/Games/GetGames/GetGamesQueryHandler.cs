using Teams.Core.CQRS;
using Teams.Core.Services;
using Teams.Data.Models;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.GetGames;

public class GetGamesQueryHandler(IReadOnlyGamesRepository repository, IActorAccessor actor)
    : IRequestHandler<GetGamesQuery, IReadOnlyCollection<Game>>
{
    public async Task<IReadOnlyCollection<Game>> HandleAsync(GetGamesQuery request, CancellationToken cancellationToken)
    {
        // Ownership is always resolved against the caller's own id, never a client-supplied one -
        // there's no legitimate use case here for "show me someone else's games" via this filter.
        // Left untouched (no actor.Current access) when Ownership isn't requested, so a plain
        // unfiltered browse still doesn't require actor headers at all.
        var (organiserId, userId, organiserOrPlayerId) = request.Ownership switch
        {
            GameOwnershipEnum.Organising => (actor.Current.Id, (string?)null, (string?)null),
            GameOwnershipEnum.Playing => ((string?)null, actor.Current.Id, (string?)null),
            GameOwnershipEnum.Both => ((string?)null, (string?)null, actor.Current.Id),
            _ => ((string?)null, (string?)null, (string?)null),
        };

        var games = await repository.GetAsync(
            location: request.Location,
            startTime: new RangeFilter<DateTime>(request.StartTimeFrom, request.StartTimeTo),
            duration: new RangeFilter<int>(request.DurationFrom, request.DurationTo),
            teamSize: request.TeamSize,
            status: request.Status,
            organiserId: organiserId,
            userId: userId,
            organiserOrPlayerId: organiserOrPlayerId,
            dateFilter: new DateFilter(
                new RangeFilter<DateTime>(request.CreatedFrom, request.CreatedTo),
                new RangeFilter<DateTime>(request.ModifiedFrom, request.ModifiedTo)),
            pagination: new PaginationFilter(request.Cursor, request.PageSize),
            cancellationToken: cancellationToken);

        return [.. games];
    }
}