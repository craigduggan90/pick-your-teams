using Beans.Requestable;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.RecordResult;

public record RecordGameResultCommand(string Id, string Winner) : IRequest<Game>;