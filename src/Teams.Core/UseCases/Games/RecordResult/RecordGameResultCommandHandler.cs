using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.RecordResult;

public class RecordGameResultCommandHandler(
    IUnitOfWork uow,
    IValidator<RecordGameResultCommand> validator,
    ILogger<RecordGameResultCommandHandler> logger)
    : IRequestHandler<RecordGameResultCommand, Game>
{
    public async Task<Game> HandleAsync(RecordGameResultCommand request, CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var game = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.Id);

        // Set the result & mark the game as updated
        Enum.TryParse<GameTeamEnum>(request.Winner, true, out var winner);
        game.SetResult(winner);
        await uow.Games.UpdateAsync(game, cancellationToken);

        foreach (var player in game.Players)
            await UpdatePlayerAsync(game, player, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Game result recorded: {game}", game);

        return game;
    }

    private async Task UpdatePlayerAsync(Game game, Player player, CancellationToken cancellationToken)
    {
        // No change if the player wasn't on a team
        if (player.Team == GameTeamEnum.None)
            return;

        // These values cannot be null, a they are all set by Game.SetResult, which is called before this method 
        var teamSize = player.Team == GameTeamEnum.Home ? game.HomeTeamPlayerCount : game.AwayTeamPlayerCount;
        var teamRating = player.Team == GameTeamEnum.Home ? game.HomeTeamRating : game.AwayTeamRating;
        var teamRatingChange = player.Team == GameTeamEnum.Home ? game.HomeTeamRatingChange : game.AwayTeamRatingChange;

        player.SetRatingChange(teamRating!.Value, teamRatingChange!.Value, teamSize);
        await uow.Players.UpdateAsync(player, cancellationToken);

        if (player is { RatingChange: { } ratingChange, User: not null })
            await UpdateUserAsync(player.User, ratingChange, cancellationToken);
    }

    private async Task UpdateUserAsync(User user, int ratingChange, CancellationToken cancellationToken)
    {
        user.ApplyRatingChange(ratingChange);
        if (user.IsDirty)
            await uow.Users.UpdateAsync(user, cancellationToken);
    }
}