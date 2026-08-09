using Teams.Core.CQRS;
using Teams.Core.Services;
using Teams.Data.Models;
using Teams.Data.Repositories.Games;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.GetInvitations;

public class GetInvitationsQueryHandler(
    IReadOnlyInvitationsRepository invitationsRepository,
    IReadOnlyGamesRepository gamesRepository,
    IActorAccessor actor)
    : IRequestHandler<GetInvitationsQuery, IReadOnlyCollection<Invitation>>
{
    public async Task<IReadOnlyCollection<Invitation>> HandleAsync(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId is not null)
            actor.Current.ThrowIfNotUser(request.UserId);

        if (request.GameId is not null)
            await EnsureGameOrganiserAsync(request.GameId, cancellationToken);

        var invitations = await invitationsRepository.GetInvitationsAsync(
            gameId: request.GameId,
            userId: request.UserId,
            emailAddress: request.EmailAddress,
            status: request.Status,
            dateFilter: new DateFilter(
                new RangeFilter<DateTime>(request.CreatedFrom, request.CreatedTo),
                new RangeFilter<DateTime>(request.ModifiedFrom, request.ModifiedTo)),
            pagination: new PaginationFilter(request.Cursor, request.PageSize),
            cancellationToken: cancellationToken);

        return [.. invitations];
    }

    private async Task EnsureGameOrganiserAsync(string gameId, CancellationToken cancellationToken)
    {
        var game = await gamesRepository.GetByIdAsync(gameId, cancellationToken);

        // If the game doesn't exist, it'll get no hits anyway.
        if (game is null)
            return;

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);
    }
}