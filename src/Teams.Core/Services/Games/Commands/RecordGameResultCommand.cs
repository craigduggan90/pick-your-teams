using Teams.Domain.Enums;

namespace Teams.Core.Services.Games.Commands;

public record RecordGameResultCommand(string Id, GameTeamEnum Winner);