using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.SetTeams;

public class SetTeamsCommandHandler(
    IUnitOfWork uow,
    IValidator<SetTeamsCommand> validator,
    ILogger<SetTeamsCommandHandler> logger)
    : IRequestHandler<SetTeamsCommand, Game>
{
    public async Task<Game> HandleAsync(SetTeamsCommand request, CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var game = await uow.Games.GetByIdAsync(request.GameId, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), request.GameId);

        if (game.Status == GameStatusEnum.Finished)
            throw RequestHandlerException.ForCommandRequest("Teams cannot be changed for a completed game.");

        if (request.HomeTeamIds.Count > game.TeamSize)
            throw RequestHandlerException.ForCommandRequest("Too many players provided for home team.");

        if (request.AwayTeamIds.Count > game.TeamSize)
            throw RequestHandlerException.ForCommandRequest("Too many players provided for away team.");

        List<Player> assignedPlayers = [];

        // Populate home team
        foreach (var playerId in request.HomeTeamIds)
            assignedPlayers.Add(await AssignTeam(playerId, GameTeamEnum.Home, game, cancellationToken));

        // Populate away team
        foreach (var playerId in request.AwayTeamIds)
            assignedPlayers.Add(await AssignTeam(playerId, GameTeamEnum.Away, game, cancellationToken));

        // Clear assignment from all other players
        var assignedPlayerIds = assignedPlayers.Select(p => p.Id).ToArray();
        var unassignedPlayers = game.Players
            .Where(player => !assignedPlayerIds.Contains(player.Id, StringComparer.OrdinalIgnoreCase));
        foreach (var player in unassignedPlayers)
            await UnassignTeam(player, cancellationToken);

        game.UpdateHomeTeamRating();
        game.UpdateAwayTeamRating();

        // TODO: Test the inverse route with an integration test
        if (game.IsDirty)
            await uow.Games.UpdateAsync(game, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Game updated - teams assigned: {game}", game);

        return game;
    }

    private async Task<Player> AssignTeam(string playerId, GameTeamEnum team, Game game, CancellationToken cancellationToken)
    {
        var player = game.Players.FirstOrDefault(p => p.Id.Equals(playerId, StringComparison.OrdinalIgnoreCase))
                     ?? throw new NotFoundException(typeof(Player), playerId);

        player.AssignTeam(team, player.User?.Rating ?? player.Rating);

        return player.IsDirty
            ? await uow.Players.UpdateAsync(player, cancellationToken)
            : player;
    }

    private async Task<Player> UnassignTeam(Player player, CancellationToken cancellationToken)
    {
        player.UnassignTeam();
        return player.IsDirty
            ? await uow.Players.UpdateAsync(player, cancellationToken)
            : player;
    }
}