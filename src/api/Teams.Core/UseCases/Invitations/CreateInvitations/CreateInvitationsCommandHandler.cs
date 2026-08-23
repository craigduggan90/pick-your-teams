using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Core.Services;
using Teams.Core.Services.Events;
using Teams.Data.Services;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Invitations.CreateInvitations;

public class CreateInvitationsCommandHandler(
    IUnitOfWork uow,
    IActorAccessor actor,
    IEventPublisher publisher,
    IValidator<CreateInvitationsCommand> validator,
    ILogger<CreateInvitationsCommandHandler> logger)
    : IRequestHandler<CreateInvitationsCommand>
{
    public async Task HandleAsync(CreateInvitationsCommand request, CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var game = await uow.Games.GetByIdAsync(request.GameId, cancellationToken)
                   ?? throw new NotFoundException(typeof(Game), request.GameId);

        actor.Current.ThrowIfNotOrganiser(game.OrganiserId);

        List<InvitationCreatedEvent> events = [];
        List<string> unmappedTags = [];
        foreach (var tag in request.UserTags)
        {
            var user = await uow.Users.GetByTagAsync(tag, cancellationToken);
            if (user is null)
            {
                unmappedTags.Add(tag);
                continue;
            }

            var openInvitations = await uow.Invitations.GetInvitationsAsync(
                gameId: request.GameId,
                userId: user.Id,
                status: InvitationStatusEnum.Open,
                cancellationToken: cancellationToken);
            if (openInvitations.Any())
                continue;

            var invitation = await uow.Invitations.CreateAsync(new Invitation(request.GameId, user.Id, user.EmailAddress), cancellationToken);
            events.Add(new InvitationCreatedEvent(invitation.Id, game.Id, user.Id));
        }

        if (unmappedTags.Count != 0)
        {
            throw new CommandValidationException(unmappedTags
                .Select(tag => new ValidationFailure(string.Empty, $"Tag not found: {tag}")));
        }

        // Commit the changes
        await uow.SaveChangesAsync(cancellationToken);

        // Send the notifications
        await publisher.PublishEventsAsync(events, cancellationToken);

        // Log
        logger.LogInformation("Game {id} invitations created: {userIds}", game.Id, string.Join(';', events.Select(evt => evt.UserId)));
    }
}