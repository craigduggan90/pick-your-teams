using Teams.Core.CQRS;
using Teams.Data.Models;
using Teams.Data.Repositories.Invitations;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Invitations.GetInvitations;

public class GetInvitationsQueryHandler(IReadOnlyInvitationsRepository repository)
    : IRequestHandler<GetInvitationsQuery, IReadOnlyCollection<Invitation>>
{
    public async Task<IReadOnlyCollection<Invitation>> HandleAsync(GetInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await repository.GetInvitationsAsync(
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
}