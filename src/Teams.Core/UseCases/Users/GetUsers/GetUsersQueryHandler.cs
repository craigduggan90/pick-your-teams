using Teams.Core.CQRS;
using Teams.Data.Models;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUsers;

public class GetUsersQueryHandler(IReadOnlyUsersRepository repository)
    : IRequestHandler<GetUsersQuery, IReadOnlyCollection<User>>
{
    public async Task<IReadOnlyCollection<User>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAsync(
            emailAddress: request.EmailAddress,
            tag: request.Tag,
            displayName: request.DisplayName,
            rating: new RangeFilter<int>(request.RatingFrom, request.RatingTo),
            dateFilter: new DateFilter(
                new RangeFilter<DateTime>(request.CreatedFrom, request.CreatedTo),
                new RangeFilter<DateTime>(request.ModifiedFrom, request.ModifiedTo)),
            pagination: new PaginationFilter(request.Cursor, request.PageSize),
            cancellationToken: cancellationToken);

        return [.. entities];
    }
}