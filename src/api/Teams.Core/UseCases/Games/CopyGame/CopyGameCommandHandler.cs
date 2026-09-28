using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.CopyGame;

public class CopyGameCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    ILogger<CopyGameCommandHandler> logger) : IRequestHandler<CopyGameCommand, Game>
{
    public async Task<Game> HandleAsync(CopyGameCommand request, CancellationToken cancellationToken)
    {
        var source = await uow.Games.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.Id);

        actor.Current.ThrowIfNotOrganiser(source.OrganiserId);

        if (source.Status != GameStatusEnum.Finished)
            throw new CommandValidationException([new ValidationFailure(nameof(CopyGameCommand.Id), "Only a finished game can be copied.")]);

        var copy = await uow.Games.CreateAsync(
            new Game(actor.Current.Id, source.Location, request.StartTime, source.Duration, source.TeamSize),
            cancellationToken);

        foreach (var player in source.Players)
            await CopyPlayerAsync(copy, player, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Game copied: {source} -> {copy}", source.Id, copy.Id);
        return copy;
    }

    private async Task CopyPlayerAsync(Game copy, Player player, CancellationToken cancellationToken)
    {
        if (player.Type == PlayerTypeEnum.User && player.User is not null)
        {
            await uow.Players.CreateAsync(new Player(copy, player.User), cancellationToken);
            return;
        }

        if (player.Type == PlayerTypeEnum.Dummy)
        {
            var rating = player.Rating + (player.RatingChange ?? 0);
            await uow.Players.CreateAsync(new Player(copy, player.DisplayName!, rating), cancellationToken);
            return;
        }

        logger.LogWarning("Skipped copying player {player}: associated user has been deleted.", player.Id);
    }
}