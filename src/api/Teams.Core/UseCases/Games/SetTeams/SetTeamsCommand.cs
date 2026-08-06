using Teams.Core.CQRS;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.SetTeams;

public record SetTeamsCommand(
    string GameId,
    IReadOnlyCollection<string> HomeTeamIds,
    IReadOnlyCollection<string> AwayTeamIds) : IRequest<Game>;