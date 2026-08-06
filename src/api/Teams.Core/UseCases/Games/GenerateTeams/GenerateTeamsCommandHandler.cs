using FluentValidation;
using Teams.Core.CQRS;
using Teams.Core.Exceptions;
using Teams.Data.Repositories.Games;
using Teams.Domain.Entities;
using Teams.Domain.Exceptions;
using Teams.Domain.Models;

namespace Teams.Core.UseCases.Games.GenerateTeams;

public class GenerateTeamsCommandHandler(
    IReadOnlyGamesRepository repository,
    IValidator<GenerateTeamsCommand> validator)
    : IRequestHandler<GenerateTeamsCommand, IReadOnlyCollection<TeamSuggestion>>
{
    public async Task<IReadOnlyCollection<TeamSuggestion>> HandleAsync(
        GenerateTeamsCommand request,
        CancellationToken cancellationToken)
    {
        CommandValidationException.ThrowIfValidationFailed(await validator.ValidateAsync(request, cancellationToken));

        var game = await repository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException(typeof(Game), request.GameId);

        try
        {
            return game.GetTeamSuggestions(
                homeTeamSeedIds: request.HomeSeedPlayerIds,
                awayTeamSeedIds: request.AwaySeedPlayerIds,
                differentialThreshold: request.Differential,
                maxSuggestions: request.Count);
        }
        catch (TeamGenerationException ex)
        {
            throw RequestHandlerException.ForCommandRequest(ex.Message);
        }
    }
}