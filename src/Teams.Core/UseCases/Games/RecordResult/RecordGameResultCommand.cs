using Teams.Core.CQRS;
using Teams.Domain.Entities;
using Teams.Domain.Enums;

namespace Teams.Core.UseCases.Games.RecordResult;

public record RecordGameResultCommand(string Id, GameTeamEnum Winner) : IRequest<Game>;