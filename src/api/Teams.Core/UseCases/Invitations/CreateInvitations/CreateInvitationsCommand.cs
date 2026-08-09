using Teams.Core.CQRS;

namespace Teams.Core.UseCases.Invitations.CreateInvitations;

public record CreateInvitationsCommand(string GameId, IReadOnlyCollection<string> UserTags) : IRequest;