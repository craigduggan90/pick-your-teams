using Microsoft.Extensions.Logging;
using Teams.Common.Pagination;
using Teams.Core.Exceptions;
using Teams.Core.Extensions;
using Teams.Core.Services.Players.Commands;
using Teams.Core.Services.Players.Queries;
using Teams.Data.Models;
using Teams.Data.Repositories.Players;
using Teams.Data.Services;
using Teams.Domain.Entities;

namespace Teams.Core.Services.Players;

public class PlayersService(
    IReadOnlyPlayersRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<PlayersService> logger)
    : IPlayersService
{
    public async Task<PagedList<Player>> GetPlayersAsync(GetPlayersQuery query, CancellationToken cancellationToken)
    {
        var collection = await repository.GetAsync(
            query.Name,
            new RangeFilter<int>(query.RatingFrom, query.RatingTo),
            new DateFilter(
                new RangeFilter<DateTime>(query.CreatedFrom, query.CreatedTo),
                new RangeFilter<DateTime>(query.ModifiedFrom, query.ModifiedTo)),
            new PaginationFilter(query.Cursor, query.PageSize),
            cancellationToken);

        return collection.ToPagedList();
    }

    public async Task<Player> GetPlayerByIdAsync(GetPlayerByIdQuery query, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(query.Id, cancellationToken) ??
           throw new NotFoundException(typeof(Player), query.Id);

    public async Task<Player> CreatePlayerAsync(CreatePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = new Player(command.Name, command.UserId);
        // Business layer validation?  Does the UserId have to be unique when provided/map to a real user, etc.
        // Most of that can come later though, we're good for now
        await unitOfWork.Players.CreateAsync(player, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return player;
    }

    public async Task UpdatePlayerAsync(UpdatePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await unitOfWork.Players.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), command.Id);

        player.Update(command.Name);

        await unitOfWork.Players.UpdateAsync(player, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlayerAsync(DeletePlayerCommand command, CancellationToken cancellationToken)
    {
        var player = await unitOfWork.Players.GetByIdAsync(command.Id, cancellationToken)
                     ?? throw new NotFoundException(typeof(Game), command.Id);

        player.Delete();

        await unitOfWork.Players.UpdateAsync(player, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}