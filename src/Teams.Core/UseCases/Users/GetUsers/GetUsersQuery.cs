using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Users.GetUsers;

public record GetUsersQuery(
    string? EmailAddress = null,
    string? Tag = null,
    string? DisplayName = null,
    int? RatingFrom = null,
    int? RatingTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null,
    int? PageSize = null,
    long? Cursor = null) : IRequest<IReadOnlyCollection<User>>;