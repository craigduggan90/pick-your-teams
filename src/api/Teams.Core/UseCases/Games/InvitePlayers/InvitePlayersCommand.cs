using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.InvitePlayers;

public record InvitePlayersCommand(string GameId, IReadOnlyCollection<string> UserIdentifiers)
    : IRequest<Game>;