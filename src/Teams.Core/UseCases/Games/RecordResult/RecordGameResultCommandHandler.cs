using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.RecordResult;

public class RecordGameResultCommandHandler(IUnitOfWork uow, ILogger<RecordGameResultCommandHandler> logger)
    : IRequestHandler<RecordGameResultCommand, Game>
{
    public async Task<Game> HandleAsync(RecordGameResultCommand request, CancellationToken cancellationToken)
    {
        var game = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.Id);

        // Set the result & mark the game as updated
        game.SetResult(request.Winner);
        await uow.Games.UpdateAsync(game, cancellationToken);

        foreach (var player in game.Players)
            await UpdatePlayerAsync(game, player, cancellationToken);

        return game;
    }

    private async Task UpdatePlayerAsync(Game game, Player player, CancellationToken cancellationToken)
    {
        // No change if the player wasn't on a team
        if (player.Team == GameTeamEnum.None)
            return;

        var teamRating = player.Team == GameTeamEnum.Home ? game.HomeTeamRating : game.AwayTeamRating;
        if (teamRating is null)
            return;

        var teamRatingChange = player.Team == GameTeamEnum.Home ? game.HomeTeamRatingChange : game.AwayTeamRatingChange;
        if (teamRatingChange is null)
            return;

        var teamSize = player.Team == GameTeamEnum.Home ? game.HomeTeamPlayerCount : game.AwayTeamPlayerCount;
        if (teamSize <= 0)
            return;

        player.SetRatingChange(teamRating.Value, teamRatingChange.Value, teamSize);
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