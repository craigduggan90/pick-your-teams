using Beans.Requestable;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.DeleteGame;

public record DeleteGameCommand(string Id) : IRequest<Game>;