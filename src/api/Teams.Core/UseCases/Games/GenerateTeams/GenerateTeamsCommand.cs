using Teams.Core.CQRS;
using Teams.Domain.Models;

namespace Teams.Core.UseCases.Games.GenerateTeams;

public record GenerateTeamsCommand(
    string GameId,
    IReadOnlyCollection<string> HomeSeedPlayerIds,
    IReadOnlyCollection<string> AwaySeedPlayerIds,
    int Differential,
    int Count)
    : IRequest<IReadOnlyCollection<TeamSuggestion>>;