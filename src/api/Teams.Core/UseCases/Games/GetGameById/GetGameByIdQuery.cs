using Beans.Requestable;
using Teams.Domain.Entities;

namespace Teams.Core.UseCases.Games.GetGameById;

public record GetGameByIdQuery(string Id) : IRequest<Game>;