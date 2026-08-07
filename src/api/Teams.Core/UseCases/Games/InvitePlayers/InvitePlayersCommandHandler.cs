using FluentValidation;
using Microsoft.Extensions.Logging;
using Teams.Common.Extensions;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Core.Services.Invitations;
using Teams.Data.Repositories.Games;
using Teams.Data.Repositories.Users;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.InvitePlayers;

public class InvitePlayersCommandHandler(
    IReadOnlyGamesRepository gameRepository,
    IReadOnlyUsersRepository userRepository,
    IActorAccessor actor,
    IGameInvitationDispatcher inviter,
    IValidator<InvitePlayersCommand> validator,
    ILogger<InvitePlayersCommandHandler> logger) : IRequestHandler<InvitePlayersCommand, Game>
{
    public async Task<Game> HandleAsync(InvitePlayersCommand request, CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.GameId);

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);

        foreach (var userIdentifier in request.UserIdentifiers)
            await InviteUser(game, userIdentifier, cancellationToken);

        return game;
    }

    private async Task InviteUser(Game game, string userIdentifier, CancellationToken cancellationToken)
    {
        var user = userIdentifier.IsValidTag()
            ? await userRepository.GetByTagAsync(userIdentifier, cancellationToken)
            : await userRepository.GetByEmailAddressAsync(userIdentifier, cancellationToken);

        if (user is null && userIdentifier.IsValidEmail())
        {
            await inviter.SendNewUserInvitationAsync(game, userIdentifier);
            logger.LogInformation("Game '{gameId}' invitation sent to new user '{email}'.", game.Id, userIdentifier);
        }
        else if (user is not null)
        {
            await inviter.SendExistingUserInvitationAsync(game, user.Id, user.EmailAddress);
            logger.LogInformation("Game '{gameId}' invitation sent to user '{userId}'.", game.Id, user.Id);
        }
    }
}